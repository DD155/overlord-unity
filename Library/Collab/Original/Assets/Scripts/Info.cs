using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using GameSparks.Core;
using TMPro;

public class Info : MonoBehaviour {
	/*
	 * 
	 * animations
	 * win screen
	 * dmg/heal numbers
	 * status numbers/icons
	 * bosses
	 * better balancing
	 * mission list/map
	*/
	public static readonly int RED = 0;
	public static readonly int BLUE = 1; 
	public static readonly int GREEN = 2; 
	public static readonly int WHITE = 3; 
	public static readonly int BLACK = 4;
	public static readonly string[] passives = {
		"FlatHP",//0
		"FlatATK",//1
		"FlatDef",//2
		"%Hp",//3
		"%Atk",//4
		"%Def",//5
		"%TeamHp",//6
		"%TeamAtk",//7
		"%TeamDef",//8
		"FlatTeamHp",//9
		"FlatTeamAtk",//10
		"FlatTeamDef",//11
		"FlatLifeRegen",//12
		"%ParalyseInflict",//13
		"%StunInflict",//14
		"%FreezeInflict",//15
		"%PoisonInflict",//16
		"%BurnInflict",//17
		"%ParalyseRes",//18
		"%StunRes",//19
		"%FreezeRes",//20
		"%PoisonRes",//21
		"%BurnRes",//22
		"%ElementAdv",//23
		"%MoreDamage",//24
		"%DmgReduct",//25
		"%Evasion",//26
		"%LifeRegen"//27
	};
	//0>2>1>0
	public static readonly Color[] MYCOLORS = {Color.red,Color.blue,Color.green,Color.white,Color.magenta};
	bool orbsinslot;
	public bool playerTurn;
	public double[,] allUnit;
	public int[] unitElement;
	public int bossElement;
	public int[] bossInfo;
	public int[] unitRarity;
	public int[] unitRank;
	public int totalUnitHP;
	public int[] currentRotation;
	public string bossImg;
	public string[] unitImg;
	public double[,] passiveValues;
	public int[] bossStatus;
	public double[] bossPassives;
	public int[] playerStatus;
	public int clickedUnit;
	public String[] unitNames;

	// Use this for initialization
	async void Awake () {
		//1-3
		//4-6
		//7-10
		//11-15
		GameObject.Find ("Unit1").transform.localScale = new Vector3 (0, 0, 0);
		clickedUnit=0;
		playerTurn = true;
		inputInfo ();
		await getStage ();
		int[] a = await getTeam ();
		for(int i=0;i<4;i++){
			string[] c;
			if (a [i + 1] != -1) {
				c = await getUnitInfo (await getUnit (a [i + 1], i));
			} else {
				c = await getUnitInfo (1000);
				unitRank [i] = 1;
			}
			unitRarity [i] = Int32.Parse (c [4]);
			allUnit [i, 0] = Int32.Parse (c [0]);
			allUnit [i, 1] = Int32.Parse (c [1]);
			allUnit [i, 2] = Int32.Parse (c [2]);
			unitElement [i] = Int32.Parse (c [5]);
			unitNames [i] = c [6];
			unitImg [i] = c [3];

		}
		for (int i = 0; i < 4; i++) {
			//GameObject instance = Instantiate(Resources.Load("Pictures/Status/Image", typeof(GameObject))) as GameObject;
			//GameObject ac = Instantiate (instance);
			//ac.GetComponent<UnityEngine.UI.Image> ().sprite = Resources.Load<Sprite> ("Pictures/RankBorder/grayBorder");
			//if (unitRank [i] > 3) {
			//	ac.GetComponent<UnityEngine.UI.Image> ().sprite = Resources.Load<Sprite> ("Pictures/RankBorder/blueBorder");
			//}
			//if (unitRank [i] > 6) {
			//	ac.GetComponent<UnityEngine.UI.Image> ().sprite = Resources.Load<Sprite> ("Pictures/RankBorder/purpleBorder");
			//}
			//if (unitRank [i] > 10) {
			//	ac.GetComponent<UnityEngine.UI.Image> ().sprite = Resources.Load<Sprite> ("Pictures/RankBorder/orangeBorder");
			//}
			//ac.transform.SetParent (GameObject.Find ("C" + i).GetComponentInChildren<UnityEngine.UI.Image> ().transform);
			GameObject.Find ("C" + i).GetComponentInChildren<UnityEngine.UI.Image> ().sprite = Resources.Load<Sprite> (unitImg [i]);
			GameObject.Find ("EleImage"+i).GetComponent<UnityEngine.UI.Image> ().sprite = Resources.Load<Sprite> ("Pictures/Elements/"+unitElement[i]);

		}
		GameObject.Find ("BossPanel").GetComponentInChildren<UnityEngine.UI.Image> ().sprite = Resources.Load<Sprite> (bossImg);
		calcStats ();

	}
	
	// Update is called once per frame
	void Update () {
		orbsinslot = false;
		Transform endObjects = GameObject.Find ("ComfirmOrbPanel").transform;
		foreach (Transform slot in endObjects) {
			GameObject item;
			try{
				item = slot.GetComponent<Slot> ().item;
			}catch(NullReferenceException e){
				item = null;
			}
			if (item) {
				orbsinslot = true;
			}
		}
		if (orbsinslot) {
			GameObject.Find ("ConfirmPanel").transform.localScale = new Vector3 (1, 1, 1);
			GameObject.Find ("ClearPanel").transform.localScale = new Vector3 (1, 1, 1);
		} else {
			GameObject.Find ("ConfirmPanel").transform.localScale = new Vector3 (0, 0, 0);
			GameObject.Find ("ClearPanel").transform.localScale = new Vector3 (0, 0, 0);
		}

	}

	public void inputInfo(){
		currentRotation = new int[3]{0,1,2};
		unitRarity = new int[4];
		unitRank = new int[4];
		unitElement = new int[4];
		allUnit = new double[4,3];
		passiveValues = new double[4, 28];
		unitImg = new string[4];
		bossStatus = new int[5];
		playerStatus = new int[5];
		bossPassives = new double[28];
		unitNames = new String[4];
	}

	public void calcStats(){
		for (int i = 0; i < unitRank.Length; i++) {
			allUnit [i, 0] = (int)(allUnit [i, 0] / Math.Log (2) * Math.Log (2 + unitRank [i]) + (unitRank [i] * (25 * unitRarity [i])));
			allUnit [i, 1] = (int)(allUnit [i, 1] / Math.Log (2) * Math.Log (2 + unitRank [i]) + (unitRank [i] * (25 * unitRarity [i])));
			allUnit [i, 2] = (int)(allUnit [i, 2] / Math.Log (2) * Math.Log (2 + unitRank [i]) + (unitRank [i] * (25 * unitRarity [i])));
		}
		for(int k=0;k < passiveValues.GetLength(0);k++)
			for(int l=0;l < passiveValues.GetLength(1);l++)
				passiveValues[k,l]*=(unitRarity[k]*unitRank[k]);
		for (int i = 0; i < 4; i++) {
			allUnit [i,0] *= 1+passiveValues [i, 3];
			allUnit [i,1] *= 1+passiveValues [i, 4];
			allUnit [i,2] *= 1+passiveValues [i, 5];
			for (int j = 0; j < 4; j++) {
				allUnit [j,0] *= 1+passiveValues [i, 6];
				allUnit [j,1] *= 1+passiveValues [i, 7];
				allUnit [j,2] *= 1+passiveValues [i, 8];
			}
		}
		for (int i = 0; i < 4; i++) {
			allUnit [i,0] += passiveValues [i, 0];
			allUnit [i,1] += passiveValues [i, 1];
			allUnit [i,2] += passiveValues [i, 2];
			for (int j = 0; j < 4; j++) {
				allUnit [j,0] += passiveValues [i, 9];
				allUnit [j,1] += passiveValues [i, 10];
				allUnit [j,2] += passiveValues [i, 11];
			}
		}
		totalUnitHP = 0;
		for(int i=0;i<allUnit.GetLength(0);i++){
			totalUnitHP += (int)allUnit[i, 0];
		}
		bossInfo [0] = 2000;
		bossInfo [1] = 400;
		bossInfo [2] = 170;
		GameObject.Find ("EnemyHPPanel").GetComponent<SetBossLife> ().currentHP = bossInfo [0];
		GameObject.Find ("EnemyHPPanel").GetComponent<SetBossLife> ().totalHP = bossInfo [0];
		GameObject.Find ("HPPanel").GetComponent<HumanSetHP> ().currentHP = totalUnitHP;
		GameObject.Find ("HPPanel").GetComponent<HumanSetHP> ().totalHP = totalUnitHP;
	}

	async Task<int> getStage(){
		new GameSparks.Api.Requests.LogEventRequest ().SetEventKey ("GETQUEST").Send ((response) => {
			if (!response.HasErrors) {
				Debug.Log ("Player Saved To GameSparks...");
				GSData data = response.ScriptData.GetGSData("playerLevel");
				int a=(int)data.GetInt("num");
				data = response.ScriptData.GetGSData("levelInfo");
				int [] c=new int[3]{(int)data.GetInt(a+"hp"),(int)data.GetInt(a+"atk"),(int)data.GetInt(a+"def")};
				bossInfo=c;
				bossImg=data.GetString(a+"img");
				bossElement=(int)data.GetInt(a+"element");
			} else {
				Debug.Log ("Error Saving Player Data...");
				GameObject.Find("Connection").transform.localScale = new Vector3(1,1,1);
			}
		});
		while (bossElement == -1) {
			await Task.Delay(TimeSpan.FromSeconds(0.001));
		}
		return 1;
	}

	async Task<int[]> getTeam(){
		int a = -1;
		int[] b = new int[5];
		b [4] = -2;
		new GameSparks.Api.Requests.LogEventRequest ().SetEventKey ("GETTEAM").Send ((response) => {
			if (!response.HasErrors) {
				GSData data = response.ScriptData;
				a=(int)data.GetInt("team");
				b[0]=a;
				data = response.ScriptData.GetGSData("team"+a);
				for(int i=1;i<5;i++){
					b[i]=(int)data.GetInt("slot"+(i-1));
				}
			} else {
				Debug.Log ("Error Saving Player Data...");
				GameObject.Find("Connection").transform.localScale = new Vector3(1,1,1);
			}
		});
		while (b[4] == -2) {
			await Task.Delay(TimeSpan.FromSeconds(0.001));
		}
		return b;
	}

	async Task<int> getUnit(int num,int num2){
		int a = -1;
		new GameSparks.Api.Requests.LogEventRequest ().SetEventKey ("GETPLAYERUNIT").SetEventAttribute("UNITID",num).Send ((response) => {
			if (!response.HasErrors) {
				GSData data = response.ScriptData.GetGSData("unit"+num);
				a=(int)data.GetInt("unitShortCode");
				unitRank[num2]=(int)data.GetInt("unitRank");
				for(int i=0;i<3;i++){
					String[] c = data.GetString("passive"+i).Split(':');
					for(int j=0;j<passives.Length;j++){
						if(passives[j]==c[0]){
							passiveValues[num2,j]=Double.Parse(c[1]);
						}
					}
				}
			} else {
				Debug.Log ("Error Saving Player Data...");
				GameObject.Find("Connection").transform.localScale = new Vector3(1,1,1);
			}
		});
		while (a == -1) {
			await Task.Delay(TimeSpan.FromSeconds(0.001));
		}
		return a;
	}

	async Task<string[]> getUnitInfo(int num){
		string[] b = new string[7];
		new GameSparks.Api.Requests.LogEventRequest ().SetEventKey ("UNITINFO").SetEventAttribute("UNITSHORTCODE",num).Send ((response) => {
			if (!response.HasErrors) {
				GSData data = response.ScriptData.GetGSData("uInfo");
				b[0]=data.GetInt("hp")+"";
				b[1]=data.GetInt("atk")+"";
				b[2]=data.GetInt("def")+"";
				b[3]=data.GetString("cardImg")+"";
				b[4]=data.GetInt("rarity")+"";
				b[5]=data.GetInt("element")+"";
				b[6]=data.GetString("name");
			} else {
				Debug.Log ("Error Saving Player Data...");
				GameObject.Find("Connection").transform.localScale = new Vector3(1,1,1);
			}
		});
		while (b[6] == null||b[5] == null||b[4] == null||b[3] == null||b[2] == null||b[1] == null||b[0] == null) {
			await Task.Delay(TimeSpan.FromSeconds(0.001));
		}
		return b;
	}

	async void reCheckStats(){
		int[] a = await getTeam ();
		for(int i=0;i<4;i++){
			string[] c;
			if (a [i + 1] != -1) {
				c = await getUnitInfo (await getUnit (a [i + 1], i));
			} else {
				c = await getUnitInfo (1000);
			}
			allUnit [i, 0] = Int32.Parse (c [0]);
			allUnit [i, 1] = Int32.Parse (c [1]);
			allUnit [i, 2] = Int32.Parse (c [2]);

		}
		for (int i = 0; i < unitRank.Length; i++) {
			allUnit [i, 0] = (int)(allUnit [i, 0] / Math.Log (2) * Math.Log (2 + unitRank [i]) + (unitRank [i] * (25 * unitRarity [i])));
			allUnit [i, 1] = (int)(allUnit [i, 1] / Math.Log (2) * Math.Log (2 + unitRank [i]) + (unitRank [i] * (25 * unitRarity [i])));
			allUnit [i, 2] = (int)(allUnit [i, 2] / Math.Log (2) * Math.Log (2 + unitRank [i]) + (unitRank [i] * (25 * unitRarity [i])));
		}
		for(int k=0;k < passiveValues.GetLength(0);k++)
			for(int l=0;l < passiveValues.GetLength(1);l++)
				passiveValues[k,l]*=(unitRarity[k]*unitRank[k]);
		for (int i = 0; i < 4; i++) {
			allUnit [i,0] *= 1+passiveValues [i, 3];
			allUnit [i,1] *= 1+passiveValues [i, 4];
			allUnit [i,2] *= 1+passiveValues [i, 5];
			for (int j = 0; j < 4; j++) {
				allUnit [j,0] *= 1+passiveValues [i, 6];
				allUnit [j,1] *= 1+passiveValues [i, 7];
				allUnit [j,2] *= 1+passiveValues [i, 8];
			}
		}
		for (int i = 0; i < 4; i++) {
			allUnit [i,0] += passiveValues [i, 0];
			allUnit [i,1] += passiveValues [i, 1];
			allUnit [i,2] += passiveValues [i, 2];
			for (int j = 0; j < 4; j++) {
				allUnit [j,0] += passiveValues [i, 9];
				allUnit [j,1] += passiveValues [i, 10];
				allUnit [j,2] += passiveValues [i, 11];
			}
		}
		int tempHP = 0;
		for(int i=0;i<allUnit.GetLength(0);i++){
			tempHP += (int)allUnit[i, 0];
		}
		int tempDiff = tempHP - GameObject.Find ("HPPanel").GetComponent<HumanSetHP> ().totalHP;
		totalUnitHP += tempDiff;
		if (totalUnitHP < 1)
			totalUnitHP = 1;
		GameObject.Find ("HPPanel").GetComponent<HumanSetHP> ().totalHP = tempHP;
	}

	public void setCheckedUnit(){
		for (int i = 1; i < 5; i++) {
			GameObject.Find (i + "S").transform.localScale = new Vector3 (1, 1, 1);
			if(i!=unitRarity[clickedUnit]){
				GameObject.Find (i + "S").transform.localScale = new Vector3 (0, 0, 0);
			}
		}
		GameObject.Find ("HP(1)").GetComponent<TextMeshProUGUI> ().text = "HP: "+(int)allUnit [clickedUnit, 0];
		GameObject.Find ("ATK(1)").GetComponent<TextMeshProUGUI> ().text = "ATK: "+(int)allUnit [clickedUnit, 1];
		GameObject.Find ("DEF(1)").GetComponent<TextMeshProUGUI> ().text = "DEF: "+(int)allUnit [clickedUnit, 2];
		GameObject.Find ("NameBG").GetComponentInChildren<TextMeshProUGUI> ().text = unitNames[clickedUnit];
		GameObject.Find ("RankBG").GetComponentInChildren<TextMeshProUGUI> ().text = unitRarity[clickedUnit]+"";
		GameObject.Find("UIMG").GetComponentInChildren<UnityEngine.UI.Image> ().sprite=Resources.Load<Sprite> (unitImg[clickedUnit]);
		GameObject.Find("ElementImg").GetComponent<UnityEngine.UI.Image> ().sprite=Resources.Load<Sprite> ("Pictures/Elements/"+unitElement[clickedUnit]);
		GameObject.Find ("HelpText").GetComponent<TextMeshProUGUI> ().text = "";
		for (int i = 0; i < passives.Length; i++) {
			if (passiveValues [clickedUnit, i] != 0) {
				GameObject.Find ("HelpText").GetComponent<TextMeshProUGUI> ().text += passives[i] + ":" + (int)passiveValues[clickedUnit, i] + "\n";
			}
		}
	}
}
