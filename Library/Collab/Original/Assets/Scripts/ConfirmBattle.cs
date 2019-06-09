using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using GameSparks.Core;
using TMPro;

public class ConfirmBattle : MonoBehaviour {

	private GameObject[] orbs;
	private GameObject can;
	private double[,] charStats;
	private double[,] passiveValues;
	private int[] bossStatus;
	private int[] playerStatus;
	private double[] bossPassives;
	private int[] boss;
	private Transform endObjects;
	private int[] currentRotation;
	private int[] unitElement;
	private int bossElement;
	private int totalUnitHP;
	private string[] unitImg;
	private bool gameOver;

	async public void Confirm(){
		gameOver = false;
		orbs = GameObject.Find ("SelectOrbPanel").GetComponent<GenerateOrbs> ().orbs;
		can = GameObject.Find ("Canvas");
		charStats = can.GetComponent<Info> ().allUnit;
		boss = can.GetComponent<Info> ().bossInfo;
		unitElement = can.GetComponent<Info> ().unitElement;
		bossElement = can.GetComponent<Info> ().bossElement;
		currentRotation = can.GetComponent<Info> ().currentRotation;
		totalUnitHP = can.GetComponent<Info> ().totalUnitHP;
		passiveValues = can.GetComponent<Info> ().passiveValues;
		bossStatus = can.GetComponent<Info> ().bossStatus;
		bossPassives = can.GetComponent<Info> ().bossPassives;
		playerStatus = can.GetComponent<Info> ().playerStatus;
		unitImg = can.GetComponent<Info> ().unitImg;
		endObjects = GameObject.Find ("ComfirmOrbPanel").transform;
		if (can.GetComponent<Info> ().playerTurn) {
			playerAtk ();
			can.GetComponent<Info> ().playerTurn = false;
			foreach (Transform slot in endObjects) {
				GameObject item;
				try {
					item = slot.GetComponent<Slot> ().item;
				} catch (NullReferenceException e) {
					item = null;
				}
				if (item) {
					Destroy (item);
				}
			}
			foreach (Transform sl in GameObject.Find ("SelectOrbPanel").transform) {
				GameObject newitem = sl.GetComponent<Slot> ().item;
				if (!newitem) {
					int rand = (int)(UnityEngine.Random.value * (orbs.Length));
					GameObject a = Instantiate (orbs [rand]);
					a.name = orbs [rand].name;
					a.transform.SetParent (sl);
				}
			}
			can.GetComponent<Info> ().playerTurn = true;
		}

	}

	async private void playerAtk(){
		int[] a = new int[4];
		int temp = 0;
		bool isStunPlayer = false;
		bool isStunBoss = false;
		for (int i = 0; i < 3; i++) {
			if (playerStatus [i] != 0)
				isStunPlayer = true;
			if (bossStatus [i] != 0)
				isStunBoss = true;
		}
		foreach (Transform slot in endObjects) {
			GameObject item;
			try {
				item = slot.GetComponent<Slot> ().item;
			} catch (NullReferenceException e) {
				item = null;
			}
			if (item!=null) {
				a [temp] = item.GetComponent<orbNum> ().num;
			} else {
				a [temp] = -1;
			}
			temp++;
		}
		for (int i = 0; i < currentRotation.Length; i++) {
			double elementavg=1;
			double elementdis=1;
			double dr = passiveValues[currentRotation [i],25];
			double moredmg = 1+passiveValues[currentRotation [i],24];
			double atkorb = 1;
			double deforb = 1;
			if (unitElement [currentRotation [i]] == bossElement) {
				elementavg = 1;
				elementdis = 1;
			} else if (unitElement [currentRotation [i]] > 2 && bossElement > 2) {
				elementavg = 1.25+passiveValues[currentRotation [i],23];
				elementdis = 0.75;
			} else if (unitElement [currentRotation [i]] > bossElement || (bossElement == 2 && unitElement [currentRotation [i]] == 0)) {
				elementavg = 1.25+passiveValues[currentRotation [i],23];
				elementdis = 0.75;
			} else if (unitElement [currentRotation [i]] < bossElement || (bossElement == 0 && unitElement [currentRotation [i]] == 2)) {
				elementavg = 0.75;
				elementdis = 1.25;
			} else {
				elementavg = 1;
				elementdis = 1;
			}
			foreach (int c in a) {
				if (c == -1) {
					deforb += 0.1;
				}
				if(c==unitElement [currentRotation [i]]){
					atkorb += 0.25;
				}
			}
			if (bossPassives [26] * 100 < UnityEngine.Random.Range (1, 100) && !isStunPlayer) {
				double dmg = (double)((((charStats [currentRotation [i], 1] * (atkorb)) - boss [2]) * elementavg) * moredmg);
				dmg *= (double)UnityEngine.Random.Range (1, 1.05f);
				if (dmg < 1)
					dmg = 1;
				for (int k = 13; k < 18; k++) {
					int rand = UnityEngine.Random.Range (1, 100);
					while (rand < (100 * (passiveValues [currentRotation [i], k] - bossPassives [k + 5])) && can.GetComponent<Info> ().bossStatus [k - 13] < 5) {
						can.GetComponent<Info> ().bossStatus [k - 13] += 1;
						rand = UnityEngine.Random.Range (1, 100);
					}
				}
				foreach (Transform ab in GameObject.Find("BossStatus").transform){
					Destroy (ab.gameObject);
				}
				GameObject instance = Instantiate(Resources.Load("Pictures/Status/Image", typeof(GameObject))) as GameObject;
				for (int k = 0; k < 5; k++) {
					for (int l = 0; l < can.GetComponent<Info> ().bossStatus [k]; l++) {
						GameObject ac = Instantiate (instance);
						ac.GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite> ("Pictures/Status/" + k);
						ac.transform.SetParent (GameObject.Find ("BossStatus").transform);
					}
				}
				can.GetComponent<Info> ().bossInfo [0] -= (int)dmg;
				GameObject f=Instantiate (GameObject.Find("DmgText"));
				f.transform.localScale = new Vector3 (1, 1, 1);
				GameObject.Find ("SlashSound").GetComponents<AudioSource> () [0].Play ();
				f.transform.position = GameObject.Find ("DmgText").transform.position;
				f.transform.SetParent (can.transform);
				f.GetComponent<TextMeshProUGUI> ().text = (int)dmg+"";
				f.GetComponent<moveText> ().real = false;
				await Task.Delay(TimeSpan.FromSeconds(1));
				if (can.GetComponent<Info> ().bossInfo [0] < 1) {
					SceneManager.LoadScene ("nextLevel");
				}
			}
			if (passiveValues [currentRotation [i], 26] * 100 < UnityEngine.Random.Range (1, 100) && !isStunBoss) {
				double bossDmg = (double)((boss [1] - (charStats [currentRotation [i], 2] * deforb)) * elementdis * (1 - dr));
				bossDmg *= (double)UnityEngine.Random.Range (1, 1.05f);
				if (bossDmg < 1)
					bossDmg = 1;
				for (int k = 13; k < 18; k++) {
					int rand = UnityEngine.Random.Range (1, 100);
					while (rand < (100 * (bossPassives [k] - passiveValues [currentRotation [i], k+5])) && can.GetComponent<Info> ().playerStatus [k - 13] < 5) {
						can.GetComponent<Info> ().playerStatus [k - 13] += 1;
						rand = UnityEngine.Random.Range (1, 100);
					}
				}
				foreach (Transform ab in GameObject.Find("HumanStatus").transform){
					Destroy (ab.gameObject);
				}
				GameObject instance = Instantiate(Resources.Load("Pictures/Status/Image", typeof(GameObject))) as GameObject;
				for (int k = 0; k < 5; k++) {
					for (int l = 0; l < can.GetComponent<Info> ().playerStatus [k]; l++) {
						GameObject ac = Instantiate (instance);
						ac.GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite> ("Pictures/Status/" + k);
						ac.transform.SetParent (GameObject.Find ("HumanStatus").transform);
					}
				}
				totalUnitHP -= (int)bossDmg;
				GameObject f=Instantiate (GameObject.Find("AllyDmgText"));
				GameObject.Find ("SlashSound2").GetComponents<AudioSource> () [0].Play ();
				f.transform.localScale = new Vector3 (1, 1, 1);
				f.transform.position = GameObject.Find ("AllyDmgText").transform.position;
				f.transform.SetParent (can.transform);
				f.GetComponent<TextMeshProUGUI> ().text = (int)bossDmg+"";
				f.GetComponent<moveText> ().real = false;
				await Task.Delay(TimeSpan.FromSeconds(1));
				can.GetComponent<Info> ().totalUnitHP=totalUnitHP;
				if (can.GetComponent<Info> ().totalUnitHP < 1) {
					gameOver = true;
					GameObject.Find ("Lose").transform.localScale = new Vector3 (1, 1, 1);

					break;
				}
			}
		}
		if (gameOver) {
			GameObject.Find ("Lose").transform.localScale = new Vector3 (1, 1, 1);
		} else {
			if (bossStatus [3] != 0) {
				can.GetComponent<Info> ().bossInfo [0] = (int)(0.9*can.GetComponent<Info> ().bossInfo [0]);
			}
			if (bossStatus [4] != 0) {
				can.GetComponent<Info> ().bossInfo [0] = (int)(0.9*can.GetComponent<Info> ().bossInfo [0]);
			}
			if (playerStatus [3] != 0) {
				can.GetComponent<Info> ().totalUnitHP = (int)(0.9*can.GetComponent<Info> ().totalUnitHP);
			}
			if (playerStatus [4] != 0) {
				can.GetComponent<Info> ().totalUnitHP = (int)(0.9*can.GetComponent<Info> ().totalUnitHP);
			}
			int totalHeal = 0;
			for (int i = 0; i < 4; i++) {
				totalHeal += (int)(passiveValues [i, 12]);
				totalHeal += (int)(passiveValues [i, 27]*GameObject.Find ("HPPanel").GetComponent<HumanSetHP> ().totalHP);
			}
			can.GetComponent<Info> ().totalUnitHP+=totalHeal;
			GameObject f=Instantiate (GameObject.Find("AllyHealText"));
			GameObject.Find ("Heal").GetComponents<AudioSource> () [0].Play ();
			f.transform.localScale = new Vector3 (1, 1, 1);
			f.transform.position = GameObject.Find ("AllyHealText").transform.position;
			f.transform.SetParent (can.transform);
			f.GetComponent<TextMeshProUGUI> ().text = (int)totalHeal+"";
			f.GetComponent<moveText> ().real = false;
			await Task.Delay(TimeSpan.FromSeconds(1));
			if (can.GetComponent<Info> ().totalUnitHP > GameObject.Find ("HPPanel").GetComponent<HumanSetHP> ().totalHP)
				can.GetComponent<Info> ().totalUnitHP = GameObject.Find ("HPPanel").GetComponent<HumanSetHP> ().totalHP;
			can.GetComponent<Info> ().bossInfo [0] += (int)(bossPassives [12]);
			can.GetComponent<Info> ().bossInfo [0] += (int)(bossPassives [27]*GameObject.Find ("EnemyHPPanel").GetComponent<SetBossLife> ().totalHP);
			int bossTotalheal = (int)(bossPassives [12]) + (int)(bossPassives [27] * GameObject.Find ("EnemyHPPanel").GetComponent<SetBossLife> ().totalHP);
			f=Instantiate (GameObject.Find("HealText"));
			GameObject.Find ("Heal").GetComponents<AudioSource> () [0].Play ();
			f.transform.localScale = new Vector3 (1, 1, 1);
			f.transform.position = GameObject.Find ("HealText").transform.position;
			f.transform.SetParent (can.transform);
			f.GetComponent<TextMeshProUGUI> ().text = (int)bossTotalheal+"";
			f.GetComponent<moveText> ().real = false;
			await Task.Delay(TimeSpan.FromSeconds(1));
			for (int i = 0; i < can.GetComponent<Info> ().bossStatus.Length; i++) {
				if (can.GetComponent<Info> ().bossStatus[i] != 0)
					can.GetComponent<Info> ().bossStatus[i]--;
			}
			for (int i = 0; i < can.GetComponent<Info> ().playerStatus.Length; i++) {
				if (can.GetComponent<Info> ().playerStatus [i] != 0)
					can.GetComponent<Info> ().playerStatus [i]--;
			}
			for (int i = 0; i < can.GetComponent<Info> ().currentRotation.Length; i++) {
				if (can.GetComponent<Info> ().currentRotation [i] == 3) {
					can.GetComponent<Info> ().currentRotation [i] = 0;
				} else {
					can.GetComponent<Info> ().currentRotation [i]++;
				}
			}
			for (int i = 0; i < 4; i++) {
				if (i != 3) {
					GameObject.Find ("C" + i).GetComponentInChildren<UnityEngine.UI.Image> ().sprite = Resources.Load<Sprite> (unitImg [currentRotation [i]]);
					GameObject.Find ("EleImage"+i).GetComponent<UnityEngine.UI.Image> ().sprite = Resources.Load<Sprite> ("Pictures/Elements/"+unitElement[currentRotation [i]]);
				} else {
					if (currentRotation [2] == 3) {
						GameObject.Find ("C" + i).GetComponentInChildren<UnityEngine.UI.Image> ().sprite = Resources.Load<Sprite> (unitImg [0]);
						GameObject.Find ("EleImage"+i).GetComponent<UnityEngine.UI.Image> ().sprite = Resources.Load<Sprite> ("Pictures/Elements/"+unitElement[0]);
					} else {
						GameObject.Find ("C" + i).GetComponentInChildren<UnityEngine.UI.Image> ().sprite = Resources.Load<Sprite> (unitImg [currentRotation [2]+1]);
						GameObject.Find ("EleImage"+i).GetComponent<UnityEngine.UI.Image> ().sprite = Resources.Load<Sprite> ("Pictures/Elements/"+unitElement[currentRotation [2]+1]);
					}
				}
			}
			can.GetComponent<Info> ().reCheckStats ();
		}
	}
}
