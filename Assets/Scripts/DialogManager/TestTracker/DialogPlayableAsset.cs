using UnityEngine;
using UnityEngine.Playables;

public class DialoguePlayableAsset : PlayableAsset
{
    // Kita pecah 'EnhancedDialogLine' menjadi field individual di sini
    // SpeakerTarget sekarang menggunakan ExposedReference
    public ExposedReference<Transform> speakerTarget;

    [TextArea(3, 5)]
    public string dialogText;

    public bool shakeOnSpeak = false;
    // Tambahkan properti lain dari EnhancedDialogLine yang ingin Anda atur per klip di sini
    // contoh: public AudioClip customTypingSound;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DialoguePlayableBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        // Di sini, kita "menyelesaikan" referensi dan membangun objek EnhancedDialogLine secara dinamis
        EnhancedDialogLine lineData = new EnhancedDialogLine();
        
        // Mengambil objek Transform asli dari ExposedReference
        lineData.speakerTarget = speakerTarget.Resolve(graph.GetResolver()); 
        
        // Mengisi sisa data dari field yang ada di Inspector
        lineData.dialogText = this.dialogText;
        lineData.shakeOnSpeak = this.shakeOnSpeak;
        // lineData.customTypingSound = this.customTypingSound; // contoh

        // Mengirim data yang sudah lengkap ke behaviour
        behaviour.dialogLine = lineData;

        return playable;
    }
}