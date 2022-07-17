using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using Synthesis.Runtime;
using System.Collections.Generic;
using Synthesis.PreferenceManager;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;
public static class Shooting
{
    private const float timeBetweenShots = 0.5f;
    private static bool canShoot = true;
    
    
    private static Queue<ShootableGamepiece> shootingQueue = new Queue<ShootableGamepiece>();

    public const string TOGGLE_SHOOT_GAMEPIECE = "input/shoot-gamepiece";

    public static void Start()
    {
        
        InputManager.AssignValueInput(TOGGLE_SHOOT_GAMEPIECE, TryGetSavedInput(TOGGLE_SHOOT_GAMEPIECE, new Digital("Space", context: SimulationRunner.RUNNING_SIM_CONTEXT)));
    }
    private static Analog TryGetSavedInput(string key, Analog defaultInput)
    {
        if (PreferenceManager.ContainsPreference(key))
        {
            var input = (Digital)PreferenceManager.GetPreference<InputData[]>(key)[0].GetInput();
            input.ContextBitmask = defaultInput.ContextBitmask;
            return input;
        }
        return defaultInput;
    }

    public static void ConfigureGamepieces()
    {
        //loop through all gamepieces and attach objects
        //call this from the mode manager
        
        FieldSimObject.CurrentField.Gamepieces.ForEach(gp =>
        {
            //gp.GamepieceObject.GetComponentInChildren<MeshRenderer>().gameObject.AddComponent<ShootableGamepiece>();
            gp.GamepieceObject.AddComponent<ShootableGamepiece>();
        });
    }
    
    
    public static void Update()
    {
        //call this from the mode manager update loop
        //update the current time, detect input, and perform shooting actions
        
        bool shootGamepiece = InputManager.MappedValueInputs[TOGGLE_SHOOT_GAMEPIECE].Value == 1.0F;
        //Debug.Log($"Key: {shootGamepiece} | shooting q {shootingQueue.Count} | can shoot {canShoot}");
        
        if(shootingQueue.Count > 0 && shootGamepiece && canShoot)
        {
            canShoot = false;
            ShootGamepiece();
            MonoBehaviour _mb = GameObject.FindObjectOfType<MonoBehaviour>();
            if (_mb != null)
            {
                Debug.Log("Found a MonoBehaviour.");
                _mb.StartCoroutine(WaitForNextShot());
            }
        }

    }

    public static IEnumerator WaitForNextShot()
    {
        yield return new WaitForSeconds(timeBetweenShots);
        canShoot = true;
    }
    
    
    private static float shootForce = 70.0f;
    private static float upwardsForce = 70.0f;
    private static Vector3 verticalOffset = new Vector3(0, 1.5f, 0);
    public static GameObject intakeObject { get; set; }
    public static void ShootGamepiece()
    {
        //shoot gamepiece from queue
        Vector3 horizontal = Vector3.Normalize(intakeObject.transform.forward) * shootForce;
        Vector3 vertical = Vector3.Normalize(intakeObject.transform.up) * upwardsForce;
        Vector3 spawnPoint = intakeObject.transform.position + verticalOffset;
        Debug.Log("Spawn point: " + spawnPoint);

        if (shootingQueue.Count > 0)
            shootingQueue.Dequeue().OnShoot(horizontal,vertical, spawnPoint, intakeObject.transform.rotation.eulerAngles);
    }

    public static void AddGamepiece(ShootableGamepiece shootable)
    {
        //add gamepiece to shooting queue
        shootingQueue.Enqueue(shootable);
        
    }
    
    public static void Reset()
    {
        //clear gamepieces from queue and reset each one
        shootingQueue.ForEach(gp =>
        {
            gp.ResetGamepiece();
        });
        shootingQueue.Clear();

    }
}
