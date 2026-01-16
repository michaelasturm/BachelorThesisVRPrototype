using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.VRTemplate
{
    /// <summary>
    /// Controls the steps in the in coaching card.
    /// </summary>
    public class StepManager : MonoBehaviour
    {
        [Serializable]
        class Step
        {
            [SerializeField]
            public GameObject stepObject;

            [SerializeField]
            public string buttonText;
        }

        [SerializeField]
        public GameObject tutorialUI;
        [SerializeField]
        public TextMeshProUGUI m_StepButtonTextField;

        [SerializeField]
        List<Step> m_StepList = new List<Step>();

        [SerializeField]
        Slider tutorialSlider;
        public GameObject gazeManager;

        int m_CurrentStepIndex = 0;

        public void Next()
        {
            if (m_CurrentStepIndex == 2)
            {
                if (tutorialSlider.value >= 75)
                {
                    m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);
                    m_CurrentStepIndex = (m_CurrentStepIndex + 1) % m_StepList.Count;
                    m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
                    m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;
                    gazeManager.GetComponent<GazeManager>().setTutorialEnd();
                }
            }else
            {
                m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);
                m_CurrentStepIndex = (m_CurrentStepIndex + 1) % m_StepList.Count;
                m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
                m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;
            }

        }
    }
}
