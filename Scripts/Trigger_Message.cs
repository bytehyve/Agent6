using UnityEngine;

/// <summary>
/// Derives from <see cref="PlayerTrigger"/>. Displays a message when player collides.
/// </summary>
public class Trigger_Message : PlayerTrigger
{
    [SerializeField] private string displayMessage;

    // Use this to randomly add [randomString].[randomFileExtension] behind the message a [randomInt] amount of times.
    [Tooltip("Keep <= 0 to disable")]
    [SerializeField] private int multipleTypeVariants; 

    protected override void Execute() {
        base.Execute();
        
        if (multipleTypeVariants > 0) {
            for (int i = 0; i < multipleTypeVariants; i++) {
                string fileName = ""; string st = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                int fileNameLen = Random.Range(5, 10);
                for (int j = 0; j < fileNameLen; j++) {
                    fileName += st.ToCharArray()[Random.Range(0, st.Length)];
                }
                
                string[] types = new string[] {
                    ".dat", ".json", ".exe", ".txt", ".png", ".lock", ".dll", ".xml", ".log"
                };

                PlayerController.instance.DisplayMessage(displayMessage + $" {fileName}{types[Random.Range(0, types.Length)]}", 3.5f);
            }
        }
        else {
            PlayerController.instance.DisplayMessage(displayMessage, 3.5f);
        }
        enabled = false;
    }
}
