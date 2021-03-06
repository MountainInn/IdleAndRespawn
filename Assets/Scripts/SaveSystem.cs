using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using System.IO;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

public class SaveSystem : MonoBehaviour
{
	static SaveSystem inst;
	static public SaveSystem _Inst => inst ??= GameObject.FindObjectOfType<SaveSystem>();
    static public Action onTheFirstLaunch, onAfterLoad;

    string savePath;

    FileWriter fileWriter;

    IEnumerable<JToken> talents;

	void Awake()
	{
		savePath = Application.persistentDataPath + "/save.json";
	}

	public void SaveGame()
	{
		StringBuilder sb = new StringBuilder();

		StringWriter sw = new StringWriter(sb);

		using (JsonWriter writer = new JsonTextWriter(sw))
		{
			writer.WriteStartObject();

			var settings = new JsonSerializerSettings() { Formatting = Formatting.Indented, TypeNameHandling = TypeNameHandling.None };
			var talentSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented, TypeNameHandling = TypeNameHandling.Objects };

			Surround("Hero", JsonConvert.SerializeObject(Hero._Inst, Formatting.Indented));
			Surround("Boss", JsonConvert.SerializeObject(Boss._Inst, Formatting.Indented));
			Surround("Followers", JsonConvert.SerializeObject(Followers._Inst, Formatting.Indented));
			Surround("SoftReset", JsonConvert.SerializeObject(SoftReset._Inst, Formatting.Indented));
			Surround("Phases", JsonConvert.SerializeObject(Phases._Inst, talentSettings));
			Surround("Vault", JsonConvert.SerializeObject(Vault._Inst, settings));
			Surround("AdProgression", JsonConvert.SerializeObject(AdProgression._Inst, settings));
			Surround("Tutorial", JsonConvert.SerializeObject(Tutorial._Inst, settings));
			Surround("BattleExpiriense", JsonConvert.SerializeObject(BattleExpiriense._Inst, settings));
			Surround("OfflineIncome", JsonConvert.SerializeObject(OfflineIncome._Inst, settings));
            Surround("AdCharges", JsonConvert.SerializeObject(AdCharges._Inst, settings));

			writer.WriteEndObject();


			if (File.Exists(savePath)) File.Delete(savePath);

			File.WriteAllText(savePath, sb.ToString());


			void Surround(string objectName, string json)
			{
				writer.WritePropertyName(objectName);

				writer.WriteRawValue(json);
			}
		}
	}

	public void LoadGame()
	{
		if (File.Exists(savePath))
		{
			string json = File.ReadAllText(savePath);

			JObject save = JObject.Parse(json);

			JsonConvert.PopulateObject(save["Hero"].ToString(), Hero._Inst);
			JsonConvert.PopulateObject(save["Boss"].ToString(), Boss._Inst);
			JsonConvert.PopulateObject(save["Followers"].ToString(), Followers._Inst);

			talents = save["Phases"]["allTalents"].Children();

			LoadTalents();

			JsonConvert.PopulateObject(save["Tutorial"].ToString(), Tutorial._Inst);
			JsonConvert.PopulateObject(save["SoftReset"].ToString(), SoftReset._Inst);
			JsonConvert.PopulateObject(save["Vault"].ToString(), Vault._Inst);
			JsonConvert.PopulateObject(save["AdProgression"].ToString(), AdProgression._Inst);
			JsonConvert.PopulateObject(save["BattleExpiriense"].ToString(), BattleExpiriense._Inst);
			JsonConvert.PopulateObject(save["AdCharges"].ToString(), AdCharges._Inst);
			JsonConvert.PopulateObject(save["OfflineIncome"].ToString(), OfflineIncome._Inst);
		}
		else
		{
			onTheFirstLaunch?.Invoke();
		}


		onAfterLoad?.Invoke();
	}

	void LoadTalents()
	{
		var allTalents = new List<Talent>(Phases._Inst.allTalents);

		foreach (var item in talents)
		{
			var typeName = item["$type"].Value<string>().Split(',')[0];

			var loadedTalent = allTalents.FirstOrDefault(t => t.GetType().Name == typeName);


			if (loadedTalent != default)
			{
				JsonConvert.PopulateObject(item.ToString(), loadedTalent);

				allTalents.Remove(loadedTalent);
			}
		}
	}


}
