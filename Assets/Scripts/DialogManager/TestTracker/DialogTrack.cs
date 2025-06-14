using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// Menentukan bahwa track ini akan di-bind ke EnhancedDialogManager
[TrackBindingType(typeof(EnhancedDialogManager))]
// Menentukan bahwa klip di track ini adalah DialoguePlayableAsset
[TrackClipType(typeof(DialoguePlayableAsset))]
public class DialogueTrack : TrackAsset
{
    // Kita tidak perlu menambahkan logic apa pun di sini
    // Kelas ini berfungsi sebagai definisi untuk Timeline
}