using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;


static public class JsonReaderExt
{
    static public string ValueStr(this JsonReader reader) => reader.Value.ToString();
}


#region Attributes

public sealed class SaveRoot : Attribute {}
public sealed class SaveClass : Attribute {}
public sealed class SaveField : Attribute {}
public sealed class SaveAuxiliaryField : Attribute {}

public sealed class SaveOverride : Attribute
{
    Type overrideType;
    
    public SaveOverride(Type overrideType)
    {
        this.overrideType = overrideType;
    }

    public object GetValue(FieldInfo fieldInfo, object parentObj)
        => overrideType.GetMethod("GetVal").Invoke(null, new object[]{ fieldInfo, parentObj });
}


static public class _SaveOverrideGameobjectName
{
    static public string GetVal(FieldInfo field, object obj)
        => ((MonoBehaviour)field.GetValue(obj)).gameObject.name;
}

static public class _OverrideTypeName
{
    static public object GetVal(FieldInfo field, object obj ) => field.FieldType.Name; 
}

public sealed class LoadOverride : Attribute
{
    Type overrideType;
    
    
    public LoadOverride(Type overrideType)
    {
        this.overrideType = overrideType;
    }

    public void SetValue(FieldInfo fieldInfo, object parentObj, object val)
        => overrideType.GetMethod("LoadVal").Invoke(null, new object[]{ fieldInfo, parentObj, val });
}


static public class _LoadOverride_Unit
{
    /// По сохраненной строке найдется нужный GameObject
    /// 
    static public void LoadVal(FieldInfo field, object parent, object val)
    {
        var unitInstances =
            SaveReflection.assembly
            .GetTypes()
            .Where(t => t.BaseType == typeof(Unit))
            .Select(t => t.GetField("_Instances").GetValue(null));

        var s = unitInstances .Select(list => ((List<Unit>) list ).First(inst => inst.gameObject.name == (string)val));
        
        
        Unit unit = s.First();
        
        field.SetValue(parent, unit);
    }
}

static public class _LoadIntoProperty
{
    static public void LoadVal(FieldInfo field, object parent, object val)
    {
        var property =
            parent.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p=>p.Name.ToLower() == field.Name.ToLower());

        if(property == default) throw new Exception("Can't find property");

        property.SetValue(parent, val);
    }
}

#endregion

public class SaveReflection : MonoBehaviour
{
    static public Assembly assembly;

    List<Type> saveRootTypes;

    List<IList> saveRootsInstancesLists;

    List<FieldNode> saveForest;
    

    string savePath;

    int saveTreeID = 0;
    
    void Awake()
    {
        savePath = Application.persistentDataPath +"/save.txt";
        
        assembly =
            AppDomain
            .CurrentDomain
            .GetAssemblies()
            .First(a => a.GetName().Name == "Assembly-CSharp");


        saveRootTypes = new List<Type>
            (
                assembly
                .GetTypes()
                .Where(
                    t =>
                    t .GetCustomAttributes(typeof(SaveRoot), true).Length > 0 &&
                    !t.IsAbstract

                )
            );


        try
        {
            saveRootsInstancesLists =
                saveRootTypes
                .Select(t => (IList)t.GetField("_Instances").GetValue(null))
                .ToList();
        }
        catch (NullReferenceException e)
        {
            Debug.LogWarning("NO _Instances on type " + e.Message);
        }
        

    }

    void Start()
    {
        ConstructSaveForest();

        // if (File.Exists(savePath))
        // {
        //     LoadGame();
        // }
    }

    void ConstructSaveForest()
    {
        saveForest = new List<FieldNode>();

        foreach (var _Instances in saveRootsInstancesLists)
        {
            foreach (var inst in _Instances)
            {
                saveForest.Add(FieldTreeBuilder.ConstructTree(inst));
            }
        }
    }




    public void SaveGame()
    {
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        

        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            writer.Formatting = Formatting.Indented;

            writer.WriteStartArray();

            foreach (var tree in saveForest)
            {
                tree.Serialize(writer);
            }

            writer.WriteEndArray();
        }


        File.WriteAllText(savePath, sb.ToString());
    }

    public void LoadGame()
    {
        string json = File.ReadAllText(savePath);
        StringReader sr = new StringReader(json);
        
        using (JsonTextReader reader = new JsonTextReader(sr))
        {
            foreach (var tree in saveForest)
            {
                tree.FromJson(reader);
            }

            
        }
    }

}


public class FieldNode 
{
    public FieldNode parent;
    
    public Type type;
    public FieldInfo fieldInfo;
    public string name;
    public object obj;
    public Dictionary<string, FieldNode> descendants;
    public Func<object> save;
    public Action<object> load;

    public FieldNode(){}

    /// Конструктор для корня
    public FieldNode(object val, FieldNode parent)
    {
        this.parent = parent;

        this.obj = val;
        type = val.GetType();

        descendants = new Dictionary<string, FieldNode>();
    }

    /// Конструктор для нода
    public FieldNode(FieldInfo fieldInfo, FieldNode parent)
    {
        this.parent = parent;

        this.fieldInfo = fieldInfo;
        type = fieldInfo.FieldType;
        name = fieldInfo.Name;

        descendants = new Dictionary<string, FieldNode>();
    }

    public void AddDescendant(FieldNode node)
    {
        descendants.Add(node.name, node);
    }

    public object GetValue() => fieldInfo.GetValue(parent.obj);

    public void SetValue(object val) => fieldInfo.SetValue(parent.obj, val);


    public bool HasSaveClasses(out FieldInfo[] saveClasses)
    {
        return FieldTreeBuilder.TryGetFieldsWithAttribute(type, typeof(SaveClass), out saveClasses);
    }

    public bool HasSaveFields(out FieldInfo[] saveFields)
    {
        return FieldTreeBuilder.TryGetFieldsWithAttribute(type, typeof(SaveField), out saveFields);
    }

    public bool HasSaveAuxilliaryFields(out FieldInfo[] saveFields)
    {
        return FieldTreeBuilder.TryGetFieldsWithAttribute(type, typeof(SaveAuxiliaryField), out saveFields);
    }

    public void InitValue(object parentObj)
    {
        bool
            hasSaveOverride = fieldInfo.HasAttribute(out SaveOverride saveOverride),

            hasLoadOverride = fieldInfo.HasAttribute(out LoadOverride loadOverride)
            ;

        if (hasSaveOverride)
        {
            obj = saveOverride.GetValue(fieldInfo, parentObj);
            type = obj.GetType();

            save = () => saveOverride.GetValue(fieldInfo, parentObj);
        }
        else
        {
            obj = fieldInfo.GetValue(parentObj);
            save = () => fieldInfo.GetValue(parentObj);
        }

        if (hasLoadOverride)
        {
            load = (val) => loadOverride.SetValue(fieldInfo, parentObj, val);
        }
        else
        {
            load = (val) => fieldInfo.SetValue(parentObj, val);
        }
    }

    static public bool IsList(object obj) => obj.GetType().GetInterface("IList") != null;


    ///<summary>
    /// Создает JSON этого объекта
    ///</summary>
    virtual public void Serialize(JsonWriter writer)
    {
    }

    ///<summary>
    /// Создает JSON всех вложенных объектов
    ///</summary>
    virtual public void DescToJson(JsonWriter writer)
    {
        foreach (var item in descendants)
        {
            item.Value.Serialize(writer);
        }
    }


    virtual public void FromJson(JsonReader reader)
    {
        int descendantsLeftToLoad = descendants.Count; 

        string readerString = string.Empty;
        do
        {
            reader.Read();
            if (reader.Value != null)
                readerString = reader.ValueStr();
            else continue;
        }
        while (!(reader.TokenType == JsonToken.PropertyName &&
                 descendants.ContainsKey(readerString)) &&
               descendantsLeftToLoad > 0);

        descendants[readerString].FromJson(reader);

        descendantsLeftToLoad--;
    }

}


public class SaveRootNode : FieldNode
{
    public SaveRootNode(object obj, FieldNode parent) : base(obj, parent)
    {}

    public SaveRootNode(FieldInfo fieldInfo, FieldNode parent) :base(fieldInfo, parent)
    {}

    override public void Serialize(JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(type.Name);

        writer.WriteStartObject();

        if (descendants != null)
        {
            DescToJson(writer);
        }

        writer.WriteEndObject();

        writer.WriteEndObject();
    }

}


public class SaveListNode : FieldNode
{
    Type genericListType;
    
    public SaveListNode(object obj, FieldNode parent) : base(obj, parent)
    {}

    public SaveListNode(FieldInfo fieldInfo, FieldNode parent) :base(fieldInfo, parent)
    {
        genericListType = fieldInfo.FieldType;
    }

    override public void Serialize(JsonWriter writer)
    {
        writer.WritePropertyName(name);

        writer.WriteStartObject();


        if (descendants != null)
            DescToJson(writer);


        writer.WriteEndObject();
    }
}

public class SaveElementNode : FieldNode
{
    public SaveElementNode(object obj, FieldNode parent) : base(obj, parent)
    {}

    public SaveElementNode(FieldInfo fieldInfo, FieldNode parent) :base(fieldInfo, parent)
    {}

    override public void Serialize(JsonWriter writer)
    {
        if (fieldInfo == null)
            writer.WritePropertyName(type.Name);
        else
            writer.WritePropertyName(fieldInfo.Name);

        writer.WriteStartObject();
        
        if (descendants != null)
            DescToJson(writer);


        writer.WriteEndObject();

    }
}

public class SaveClassNode : FieldNode
{
    public SaveClassNode(object obj, FieldNode parent) : base(obj, parent)
    {}

    public SaveClassNode(FieldInfo fieldInfo, FieldNode parent) :base(fieldInfo, parent)
    {}


    override public void Serialize(JsonWriter writer)
    {
        if (fieldInfo == null)
            writer.WritePropertyName(type.Name);
        else
            writer.WritePropertyName(fieldInfo.Name);

        writer.WriteStartObject();


        if (descendants != null)
            DescToJson(writer);


        writer.WriteEndObject();

    }
}

public class SaveFieldNode : FieldNode
{
    public SaveFieldNode(object obj, FieldNode parent) : base(obj, parent)
    {}

    public SaveFieldNode(FieldInfo fieldInfo, FieldNode parent) :base(fieldInfo, parent)
    {}

    
    override public void Serialize(JsonWriter writer)
    {
        writer.WritePropertyName(fieldInfo.Name);

        writer.WriteValue(save());
    }
    
    
    override public void DescToJson(JsonWriter writer) {}

    override public void FromJson(JsonReader reader)
    {
        while(reader.TokenType == JsonToken.PropertyName)
        {
            reader.Read();
        }

            var val = reader.Value;


            if (val.GetType() == typeof(long))
            {
                load(Convert.ToInt32(val));
                Debug.Log("TOINT");
            }
            else if (val.GetType() == typeof(double))
            {
                load(Convert.ToSingle(val));
                Debug.Log("TOSINGLE");

            }
    }
}

class FieldTreeBuilder
{
    static void HandleList(FieldNode parent, FieldNode child, Stack<FieldNode> stack)
    {
        var genericType = child.type.GetGenericArguments()[0];

        var list = (IList)child.obj;

        foreach (var element in list)
        {
            var elType = element.GetType();

            bool
                isSaveClass = elType.GetCustomAttribute(typeof(SaveClass), true) != null,

                hasSaveFields = TryGetFieldsWithAttribute(elType, typeof(SaveField), out FieldInfo[] elSaveFields);


            if ( isSaveClass && hasSaveFields )
            { 
                var descendant = new SaveElementNode(element, parent);

                descendant.name = elType.Name;

                child.AddDescendant(descendant);

                stack.Push(descendant);
            }
        }

        parent.AddDescendant(child);
    }

    static public bool TryGetFieldsWithAttribute(Type type, Type attributeType, out FieldInfo[] nodes)
    {
        var  fields =
            type
            .GetFields(BindingFlags.Instance |
                       BindingFlags.Public |
                       BindingFlags.NonPublic |
                       BindingFlags.Static);

        nodes =
            fields
            .Where(
                f =>
                f.GetCustomAttributes(attributeType, true).Length > 0 ||
                f.FieldType.GetCustomAttributes(attributeType, true).Length > 0
            )
            .ToArray();


        return nodes.Length > 0;
    }

    static void HandleSaveFields(FieldNode parent, FieldInfo[] fields, Stack<FieldNode> stack)
    {
        foreach (var element in fields)
        {
            FieldNode child;

            bool isList = FieldNode.IsList(element.GetValue(parent.obj));

            if (isList) child = new SaveListNode(element, parent);
            else child = new SaveFieldNode(element, parent);

            child.InitValue(parent.obj);

            if (child.obj == null) continue;


            if (isList)
            {
                HandleList(parent, child, stack);
            }
            else
            {
                parent.AddDescendant(child);
            }
        }
    }

    static void HandleSaveClasses(FieldNode parent, FieldInfo[] fields, Stack<FieldNode> stack)
    {
        foreach (var element in fields)
        {
            FieldNode child = new SaveClassNode(element, parent);

            child.InitValue(parent.obj);

            if (child.obj == null) continue;


            stack.Push(child);

            parent.AddDescendant(child);
        }
    }

    static public FieldNode ConstructTree(object rootVal)
    {
        FieldNode rootNode = new SaveRootNode(rootVal, null);
        
        Stack<FieldNode> stack = new Stack<FieldNode>();
        stack.Push(rootNode);


        while (stack.Any())
        {
            FieldNode parent = stack.Pop();

            /// Fields
            if (parent.HasSaveFields(out FieldInfo[] saveFields))
            {
                HandleSaveFields(parent, saveFields, stack);

                if (parent.HasSaveAuxilliaryFields(out FieldInfo[] saveAuxFields))

                    HandleSaveFields(parent, saveAuxFields, stack);
            }
            
            /// Classes
            if (parent.HasSaveClasses(out FieldInfo[] saveClasses))

                HandleSaveClasses(parent, saveClasses, stack);
            
        }

        return rootNode;
    }



}

public class Skewer<T> where T : class
{
    List<T> list;
    public T current;
    
    public Skewer()
    {
        list = new List<T>();
    }

    public void Push(T obj)
    {
        list.Add(obj);
        current = obj ;
    }

    public void Pop()
    {
        list.RemoveAt(list.Count-1);

        if (list.Count == 0) current = null;
        else current = list.Last();
    }
}

public class LimitQueue<T> : Queue<T>
{
    public int limit;

    public LimitQueue(int limit) : base(limit)
    {
        this.limit = limit;
    }

    public new void Enqueue(T item)
    {
        while (Count >= limit)
        {
            Dequeue();
        }


        base.Enqueue(item);
    }

    public bool CompareSequence(LimitQueue<T> other)
    {
        if (limit != other.limit) 
        {
            Debug.LogWarning("Different limits of LimitQueues");

            return false;
        }

        bool res = true;

        T[]
            thisList = ToArray(),
            otherList = other.ToArray();

        for (int i = 0; i < limit; i++)
        {
            res &= thisList[i].Equals(otherList[i]);
        }

        return res;
    }
}


public class LimitString
{
    protected string str;
    protected int limit;

    public LimitString(int limit)
    {
        this.limit = limit;
    }

    public void Add(char c)
    {
        str += c;
        
        int dif = str.Length - limit;

        if (dif > 0) str = str.Substring(dif, limit);
    }

    public bool Compare(string other)
    {
        /// "onv"
        ///  "nv"
        ///"nonv"

        string
            thisVal = str,
            otherVal = other;

        if (str.Length > other.Length)
        {
            int dif = str.Length - other.Length;
            
            thisVal = thisVal.Substring(dif, other.Length);
        }

        return thisVal == otherVal;
    }
}


