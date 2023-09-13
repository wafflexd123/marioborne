using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimeslowDeflect : MonoBehaviour, IPlayerPower
{
    public float timeScaleSpeed, scaleDuration, recoveryTime;
    [SerializeField] ReflectWindow reflectWindowPrefab;
    Coroutine crtSlow;
    Image imgTimeleft;
    Player player;

    public bool CanDisable => crtSlow == null;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        imgTimeleft = player.transform.Find("UI").Find("Deflect Time").GetComponent<Image>();
        //reflectWindowPrefab = Instantiate(reflectWindowPrefab).Initialise(player);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Ability")) Slow();
    }

    void Slow()
    {
        if (crtSlow == null) crtSlow = StartCoroutine(E());//if not already slowing
        IEnumerator E()
        {
            float timer = 0;
            recoveryTime = 3f;
            imgTimeleft.gameObject.SetActive(true);
            //player.model.deflect = true;
            //reflectWindowPrefab.enabled = true;
            while (Input.GetButton("Ability") && timer < scaleDuration)//slow time until timer has elapsed or button is released
            {
                recoveryTime += Time.unscaledDeltaTime;
                if (Time.timeScale > Time.minTimeScale)
                {
                    Time.timeScale -= timeScaleSpeed * Time.deltaTime;
                    if (Time.timeScale < Time.minTimeScale) Time.timeScale = Time.minTimeScale;
                }
                timer += Time.unscaledDeltaTime;
                if (timer > scaleDuration) timer = scaleDuration;
                imgTimeleft.fillAmount = timer / scaleDuration;
                yield return null;
            }

            //reflectWindowPrefab.enabled = false;
            timer = 0;
            while (Time.timeScale < 1)//unslow time
            {
                Time.timeScale += timeScaleSpeed * Time.deltaTime;
                if (Time.timeScale > 1) Time.timeScale = 1;
                timer += Time.unscaledDeltaTime;//increment recovery timer
                if (timer > recoveryTime) timer = recoveryTime;
                imgTimeleft.fillAmount = 1 - timer / recoveryTime;
                yield return null;
            }

            while (timer < recoveryTime)//continue incrementing recovery timer
            {
                timer += Time.unscaledDeltaTime;
                if (timer > recoveryTime) timer = recoveryTime;
                imgTimeleft.fillAmount = 1 - timer / recoveryTime;
                yield return null;
            }

            imgTimeleft.gameObject.SetActive(false);
            crtSlow = null;
        }
    }

    //public void Deflect()
    //{
    //	if (crtDeflect == null) crtDeflect = StartCoroutine(E());
    //	IEnumerator E()
    //	{
    //		player.model.deflect = true;
    //		reflectWindowPrefab.enabled = true;
    //		yield return new WaitForSeconds(deflectTime);
    //		reflectWindowPrefab.enabled = false;

    //		deflectPercent.gameObject.SetActive(true);
    //		for (float timer = 0; timer < deflectDelay; timer += Time.fixedDeltaTime)
    //		{
    //			deflectPercent.fillAmount = timer / deflectDelay;
    //			yield return new WaitForFixedUpdate();
    //		}
    //		deflectPercent.gameObject.SetActive(false);

    //		crtDeflect = null;
    //	}
    //}
}
