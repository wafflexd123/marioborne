using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class WickUI : MonoBehaviourPlus
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
new string[] {"Gravity 1, you 0."},
new string[] { "While you may think he could, John Matrix can't fly. But he should, someone", "write that down." }
	};


	public bool demoMode;
	public float demoDelay = 3, typeDelay;
	TextMeshProUGUI[] textBoxes = new TextMeshProUGUI[3];
	Transform subText;
	Coroutine crtType;
	Action onEnd;

	private void Awake()
	{
		for (int i = 0; i < 3; i++)
		{
			textBoxes[i] = transform.GetChild(i).GetComponent<TextMeshProUGUI>();
		}
		subText = transform.GetChild(3);
	}

	public void DisplayRandom(DeathType deathType = DeathType.General)
	{
		switch (deathType)
		{
			case DeathType.General:
				Display(general[UnityEngine.Random.Range(0, general.Length)]);
				break;
			case DeathType.Fall:
				Display(UnityEngine.Random.Range(0f, 1f) > .5f ? fall[UnityEngine.Random.Range(0, fall.Length)] : general[UnityEngine.Random.Range(0, general.Length)]);
				break;
			case DeathType.Bullet:
				Display(UnityEngine.Random.Range(0f, 1f) > .5f ? bullet[UnityEngine.Random.Range(0, bullet.Length)] : general[UnityEngine.Random.Range(0, general.Length)]);
				break;
			case DeathType.Melee:
				Display(UnityEngine.Random.Range(0f, 1f) > .5f ? melee[UnityEngine.Random.Range(0, melee.Length)] : general[UnityEngine.Random.Range(0, general.Length)]);
				break;
			default:
				break;
		}
	}

	public void DisplayImmediate(string[] strings, bool showSubText = true)
	{
		gameObject.SetActive(true);
		subText.gameObject.SetActive(showSubText);
		for (int f = 0; f < 3; f++)
		{
			textBoxes[f].text = f < strings.Length ? strings[f] : "";
		}
	}

	public void Display(string[] strings, Action onEnd = null, bool showSubText = true, int animateMask = 1 << 1)
	{
		gameObject.SetActive(true);
		subText.gameObject.SetActive(showSubText);
		this.onEnd?.Invoke();//call onEnd from last Display() call if it was ended early
		this.onEnd = onEnd;
		ResetRoutine(Type(), ref crtType);
		IEnumerator Type()
		{
			for (int i = 0; i < 3; i++) textBoxes[i].text = "";

			for (int i = 0; i < strings.Length; i++)
			{
				if ((animateMask & (1 << i)) == 0) textBoxes[i].text = strings[i];//lol decipher this
				else for (int ii = 0; ii < strings[i].Length; ii++)
					{
						textBoxes[i].text += strings[i][ii];
						yield return new WaitForSecondsRealtime(typeDelay);
					}
			}

			onEnd?.Invoke();
			this.onEnd = null;
		}
	}

	public void UnDisplay(int animateMask = 1 << 1)
	{
		subText.gameObject.SetActive(false);
		onEnd?.Invoke();//call onEnd from last Display() call if it was ended early
		onEnd = null;
		ResetRoutine(Type(), ref crtType);
		IEnumerator Type()
		{
			for (int i = 0; i < textBoxes.Length; i++)
			{
				if ((animateMask & (1 << i)) == 0) textBoxes[i].text = "";
				else while (textBoxes[i].text.Length > 0)
					{
						textBoxes[i].text = textBoxes[i].text[..^1];//deciper this as well
						yield return new WaitForSecondsRealtime(typeDelay);
					}
			}
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
					bool wait = true;
					Display(general[i], () => wait = false);
					yield return new WaitWhile(() => wait);
					yield return new WaitForSeconds(demoDelay);
				}

				for (int i = 0; i < bullet.Length; i++)
				{
					bool wait = true;
					Display(bullet[i], () => wait = false);
					yield return new WaitWhile(() => wait);
					yield return new WaitForSeconds(demoDelay);
				}

				for (int i = 0; i < melee.Length; i++)
				{
					bool wait = true;
					Display(melee[i], () => wait = false);
					yield return new WaitWhile(() => wait);
					yield return new WaitForSeconds(demoDelay);
				}

				for (int i = 0; i < fall.Length; i++)
				{
					bool wait = true;
					Display(fall[i], () => wait = false);
					yield return new WaitWhile(() => wait);
					yield return new WaitForSeconds(demoDelay);
				}
			}
		}
	}
}
