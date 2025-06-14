using UnityEngine;
using UnityEngine.Playables;

public class DialoguePlayableBehaviour : PlayableBehaviour
{
    public EnhancedDialogLine dialogLine;
    private EnhancedDialogManager dialogManager;
    private bool clipPlayed = false;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        dialogManager = playerData as EnhancedDialogManager;
        if (dialogManager == null) return;

        if (!clipPlayed && info.weight > 0f)
        {
            dialogManager.PlayLineFromTimeline(dialogLine);
            clipPlayed = true;
        }
    }

    // Dipanggil saat klip selesai secara normal
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (dialogManager == null) return;
        
        // Hanya sembunyikan jika klip sudah tidak aktif sepenuhnya
        if (clipPlayed && info.weight == 0f)
        {
            dialogManager.HideLineFromTimeline(dialogLine);
            clipPlayed = false;
        }
    }
    
    // Dipanggil saat timeline berhenti sepenuhnya (misal keluar Play Mode)
    public override void OnPlayableDestroy(Playable playable)
    {
        if (dialogManager == null) return;
        
        // Membersihkan bubble jika timeline dihentikan saat klip ini masih aktif
        if (clipPlayed)
        {
            dialogManager.HideLineFromTimeline(dialogLine);
            clipPlayed = false;
        }
    }
}