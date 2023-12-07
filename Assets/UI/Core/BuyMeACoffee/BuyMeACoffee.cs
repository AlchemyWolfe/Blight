using UnityEngine;
using UnityEngine.UI;

public class BuyMeACoffee : MonoBehaviour
{
    public string CoffeeName { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Button button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        Debug.Log("I'm tryin' for " + CoffeeName);
        Application.OpenURL("http://www.buymeacoffee.com/" + CoffeeName);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
