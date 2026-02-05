using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour
{

[Header("OBLIGATORIO: El tamaño del arreglo de sprites y titulo debe ser igual al del dialogo")]
    public Dialogue dialogue;
    public bool dialogoCambiante = false;

    public Expresiones expresiones=Expresiones.Normal;
    [Tooltip("Check if this gameobject should be destroy after the dialogue has ended. Default is true")]public bool removeAfterCompleted=true;
    [Tooltip("Name of the event to trigger after the dialogue is completed")] public string eventToTrigger;

   
    public virtual void TriggerDialogue()
    {
        DialogueManager.instance.StartDialogue(this, eventToTrigger, removeAfterCompleted, (int) expresiones);
        if (dialogoCambiante)
        {
            this.GetComponent<AltDialogues>().switchDialogues();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerDialogue();

            //TargetManager.instance.StartTutorial();
        }
    }

}

public enum Expresiones{
    Triste=0,
    Feliz=1,
    Normal=2,
    Asustado=3
}