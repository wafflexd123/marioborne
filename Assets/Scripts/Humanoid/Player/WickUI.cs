using System.Collections;
using TMPro;
using UnityEngine;

public class WickUI : MonoBehaviour
{
	string[][] general =
	{
new string[] {"Get back up, maybe", "you won't die", "this time." },
new string[] {"That's not the canonical ending, John Matrix can", "never die." },
new string[] {"John Matrix is amazing. His player is", "clearly not."},
new string[] {"How did you even die that time?"},
new string[] {"Skill issue."},
new string[] {"My disappointment is", "immeasurable", "and my day is ruined."},
new string[] {"Hint:", "Dying is avoidable."},
new string[] {"Hint:", "Git gud."},
new string[] {"THERE WILL BE NO WEAKNESS IN THIS DOJO."},
new string[] {"HIT RESPAWN AND TRY AGAIN RIGHT NOW."},
new string[] {"","NEVER GIVE UP!"},
new string[] {"Just give up. John Matrix is done with you."},
new string[] {"Developer Note: Lower difficulty for this player specifically."},
new string[] {"HAHAHAHAHAHAHAHAHA *Deep Breath*", "" ,"HAHAHAHAHAHAHAHA"},
new string[] {"What would Keanu think? Smh."},
new string[] {"Now the Matrix family lost a son, a father and a husband.", "How could you."},
new string[] {"Maybe do more of the live part of", "'Live, die, repeat.'"},
new string[] {"You have infinite lives, so an infinite capacity to", "FAIL."},
new string[] {"Please stop dying. - A concerned developer"},
new string[] {"Please keep dying. - A sadistic developer"},
new string[] {"Top 10 Saddest Anime Moments."},
new string[] {"I think you're the only person to die that way,", "that's impressive."},
new string[] {"And thus, the journey of the Time Puncher ends.", "Tragic."},
new string[] {"*Face Palm*"},
new string[] {"*Double Face Palm*"},
new string[] {"*Triple Face Palm*"},
new string[] {"*Quadra Face Palm*"},
new string[] {"","*PENTA FACE PALM*"},
	};

	string[][] bullet = {
new string[] {"You're supposed to", "catch bullets with your hands", "not your face."},
new string[] {"The art of dodging eludes you."},
new string[] {"You could say you just", "BIT THE BULLET.", "HAHAHAHA."},
new string[] {"Imagine getting shot."},
new string[] {"OOF, right in the face."},
new string[] {"Looking like", "swiss cheese", "there."},
new string[] {"Lore Accurate John Matrix", "wouldn't have gotten shot", "there."},
new string[] {"Hint: You should maybe not get shot."},
new string[] {"Hint: Perhaps you should dodge."},
new string[] {"Hint: Shoot back."},
new string[] {"The boom boom stick is a no no if you want to not be die die."},
new string[] {"He shot you with his gun.", "Avoid that."},
new string[] {"DODGE!"},
new string[] {"Hey that guy was shooting you!"},
	};

	string[][] melee = {
new string[] {"Defeated in hand-to-hand combat.", "Pathetic."},
new string[] {"You're supposed to be the time puncher, not the", "time punched."},
new string[] {"Got shanked in the showers,", "unlucky."},
new string[] {"Hint: You should maybe not get stabbed."},
new string[] {"Hint: Running can be a good way to not die like you just did."},
new string[] {"The time puncher just got", "time punched."},
new string[] {"Words cannot describe my disappointment."},
new string[] {"A lifetime of training and you got", "shanked."},
new string[] {"Who would win? A superhuman with time slowing abilities or a stabby red boi? The answer will disappoint you."},
	};

	string[][] fall = {
new string[] {"Wheeeeeee", "oop", "you died."},
//new string[] {"If your death was wallrun related, hold movement towards the wall you idgit."},--not sure yet
new string[] {"It's a bird, it's a plane, oop it's a stain on the sidewalk."},
new string[] {"John Matrix usually defies gravity, but he had to be nerfed so the game would be challenging."},
new string[] {"It's only a couple broken bones.", "Walk it off."},
new string[] {"Hint: There's a drop there."},
new string[] {"Next time, do a flip."},
new string[] {"A lifetime of training and you ate it into the pavement."},
new string[] { "While you may think he could, John Matrix can't fly. But he should, someone", "write that down." }
	};


	TextMeshProUGUI[] textBoxes = new TextMeshProUGUI[3];
	public bool demoMode;
	public float demoDelay = 3;

	private void Awake()
	{
		for (int i = 0; i < 3; i++)
		{
			textBoxes[i] = transform.GetChild(i).GetComponent<TextMeshProUGUI>();
		}
	}

	public void DisplayRandom(DeathType deathType = DeathType.General)
	{
		gameObject.SetActive(true);
		switch (deathType)
		{
			case DeathType.General:
				Display(general[Random.Range(0, general.Length)]);
				break;
			case DeathType.Fall:
				Display(fall[Random.Range(0, fall.Length)]);
				break;
			case DeathType.Bullet:
				Display(bullet[Random.Range(0, bullet.Length)]);
				break;
			case DeathType.Melee:
				Display(melee[Random.Range(0, melee.Length)]);
				break;
			default:
				break;
		}
	}

	public void Display(string[] strings)
	{
		for (int f = 0; f < 3; f++)
		{
			textBoxes[f].text = f < strings.Length ? strings[f] : "";
		}
	}

	IEnumerator Start()
	{
		if (demoMode)
		{
			while (true)
			{
				for (int i = 0; i < general.Length; i++)
				{
					Display(general[i]);
					yield return new WaitForSeconds(demoDelay);
				}

				for (int i = 0; i < bullet.Length; i++)
				{
					Display(bullet[i]);
					yield return new WaitForSeconds(demoDelay);
				}

				for (int i = 0; i < melee.Length; i++)
				{
					Display(melee[i]);
					yield return new WaitForSeconds(demoDelay);
				}

				for (int i = 0; i < fall.Length; i++)
				{
					Display(fall[i]);
					yield return new WaitForSeconds(demoDelay);
				}
			}
		}
	}
}
