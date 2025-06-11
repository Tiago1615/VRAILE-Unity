using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MetricsDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown questionDropdown;
    [SerializeField] private TMP_Dropdown ageDropdown;
    [SerializeField] private TMP_Dropdown genderDropdown;
    [SerializeField] private Button sendButton;

    void Start()
    {
        List<string> questions = new List<string> { "=======", "<size=8>Hipertensión</size=8>", "<size=8>Migraña</size=8>", "<size=8>Apendicitis</size=8>", "<size=8>VIH/SIDA</size=8>", "<size=8>COVID-19</size=8>", "<size=8>Bronquiolitis</size=8>"};
        List<string> ages = new List<string> {"=======", "<size=8>Entre 20 y 30</size=8>", "<size=8>Entre 30 y 40</size=8>", "<size=8>Entre 40 y 50</size=8>", "<size=8>Menos de 20</size=8>"};
        List<string> genders = new List<string> {"=======", "<size=8>Hombre</size=8>", "<size=8>Mujer</size=8>"};

        FillDropdown(questionDropdown, questions);
        FillDropdown(ageDropdown, ages);
        FillDropdown(genderDropdown, genders);
    }

    void FillDropdown(TMP_Dropdown dropdown, List<string> options)
    {
        for (int i = 0; i < options.Count; i++)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(options[i]));
        }
        dropdown.RefreshShownValue();
    }

}
