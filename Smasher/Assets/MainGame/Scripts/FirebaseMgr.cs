using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Analytics;

public class FirebaseMgr : MonoBehaviour
{
    // void Awake()
    // {
    //     FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
    //     {
    //         var dependencyStatus = task.Result;
    //         if (dependencyStatus == DependencyStatus.Available)
    //         {
    //             InitializeFirebase();
    //         }
    //         else
    //         {
    //             UnityEngine.Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
    //         }
    //     });
    // }

    // void Update()
    // {

    // }

    // void InitializeFirebase()
    // {
    //     FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
    // }
}
