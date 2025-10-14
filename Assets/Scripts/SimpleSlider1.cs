using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SimpleSlider : MonoBehaviour
{
    public Slider slider;      // assign your slider in inspector
    public float moveSpeed = 20f; // how fast the slider moves
    public Text valueText;     // assign a UI Text to display the value

    public void OnSliderChanged(float value)
   {
       // This method is called when the slider value changes
       valueText.text = value.ToString();
    }
   
}
