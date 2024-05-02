using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using HumanAPI;
using UnityEngine;

namespace spsp
{
        [BepInPlugin("com.kirisoup.hff.spsp", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
        [BepInProcess("Human.exe")]
        public partial class Plugin : BaseUnityPlugin{

                readonly Harmony harmony = new("com.kirisoup.hff.spsp");

                public static bool isEnabled;

                public void Awake() => harmony.PatchAll();

                public void OnDestroy() => harmony.UnpatchSelf();

                public void Start() {

                        Command.Register();
                }

                public void Update() {

                        if (!Input.GetKey(KeyCode.P)) return;

                        if (Input.GetKeyDown(KeyCode.L)) CreateSphereSample(Human.Localplayer.transform.position, type: AnchorType.localplayer);

                        if (Input.GetKeyDown(KeyCode.N)) CreateSphereSample(Human.Localplayer.transform.position);

                        if (Input.GetKeyDown(KeyCode.C)) ClearSphereSamples();
                }


                public void OnGUI() {

                        RenderSphereSamples();
                }

                static void RenderSphereSamples() {

                        Plane plane = new(Camera.main.transform.forward, Camera.main.transform.position);

                        if (SphereSamples.Count == 0) return;

                        List<Vector3> points = new();

                        foreach (var sample in SphereSamples) {

                                foreach (var hit in sample.Hits) {

                                        if (pointBehindCam(hit.point, plane)) continue;

                                        if (hit.collider.transform.IsChildOf(Human.Localplayer.transform)) continue;


                                        Vector3 screen = Camera.main.WorldToScreenPoint(hit.point);

                                        Vector2 pos = new(screen.x, screen.y);

                                        Rect rect = new(pos.x, - pos.y + Screen.height, 100, 100);

                                        GUIStyle style = new();

                                        if (!hit.collider.isTrigger) style.normal.textColor = Color.white;

                                        else {

                                                if (hit.collider.gameObject.GetComponent<Checkpoint>() is not null) 
                                                        style.normal.textColor = new(1, 0.66f, 0);

                                                else if (hit.collider.gameObject.GetComponent<LevelPassTrigger>() is not null) 
                                                        style.normal.textColor = Color.green;

                                                else if (hit.collider.gameObject.GetComponent<FallTrigger>() is not null) 
                                                        style.normal.textColor = Color.red;

                                                else style.normal.textColor = Color.magenta;

                                        }

                                        GUI.Label(rect, "+", style);
                                }
                        }
                }

                static bool pointBehindCam(Vector3 point, Plane plane)
                {
                        return plane.GetDistanceToPoint(point) < 0;
                }

                public static List<SphereSample> SphereSamples = new();

                public static void CreateSphereSample(Vector3 origin, float? radius = null, int? count = null, AnchorType type = AnchorType.stationary) {

                        var sample = new SphereSample() { Origin = origin, Type = type };
                        
                        if (radius is not null) sample.Radius = (float)radius;

                        if (count is not null) sample.Count = (int)count;

                        SphereSamples.Add(sample);
                }

                public static void ClearSphereSamples() => SphereSamples = new();

                public class SphereSample {

                        Vector3 o;
                        Vector3[] e;
                        float r = 7.5f;
                        int n = 256;

                        RaycastHit[] hits;

                        AnchorType type = AnchorType.localplayer;


                        public AnchorType Type {
                                get => type;
                                set => type = value;
                        }

                        public Vector3 Origin {
                                get => type switch {
                                        AnchorType.stationary => o,
                                        AnchorType.localplayer => o = Human.Localplayer.transform.position,
                                        _ => new(),
                                };
                                set => o = value;
                        }

                        public Vector3[] Ends {
                                get => type switch {
                                        AnchorType.stationary => e ??= GenEnds(),
                                        AnchorType.localplayer => e = GenEnds(),
                                        _ => new Vector3[1] { new() },
                                };
                        }

                        public float Radius {
                                get => r;
                                set => r = value;
                        }

                        public int Count {
                                get => n;
                                set => n = value;
                        }

                        public RaycastHit[] Hits {
                                get => type switch {
                                        AnchorType.stationary => hits ??= GenHits(),
                                        AnchorType.localplayer => hits = (Human.Localplayer.velocity.sqrMagnitude >= 0.5) ? GenHits() : hits,
                                        _ => new RaycastHit[1] { new() },
                                };
                        }

                        // public bool SampleCollidable { get; set; }
                        // public bool SampleCP { get; set; }
                        // public bool SamplePTrig { get; set; }
                        // public bool SampleFTrig { get; set; }
                        // public bool SampleMiscTrig { get; set; }

                        Vector3[] GenEnds() {

                                List<Vector3> points = new();

                                for (int i = 0; i < Count; i++) {

                                        Vector3 p = DistriPointOnSphere(Origin, Radius, i, n);

                                        points.Add(p);
                                }

                                return points.ToArray();
                        }

                        RaycastHit[] GenHits() {

                                List<RaycastHit> hitlist = new();

                                foreach (var p in Ends) {

                                        var dir = (Origin - p).normalized;

                                        var hits = Physics.RaycastAll(new Ray(Origin, dir), r);

                                        foreach (var hit in hits) {

                                                if (hit.collider.transform.IsChildOf(Human.Localplayer.transform)) continue;

                                                hitlist.Add(hit);
                                        }
                                }

                                return hitlist.ToArray();
                        }

                        static Vector3 DistriPointOnSphere(Vector3 o, float r, int i, int n) {

                                float epsilon = 3.33f;

                                float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;

                                float phi = Mathf.Acos(1 - 2 * (i + epsilon) / (n - 1 + 2 * epsilon));

                                float theta = 2 * Mathf.PI * i / goldenRatio;

                                float x = r * Mathf.Cos(theta) * Mathf.Sin(phi);
                                float y = r * Mathf.Sin(theta) * Mathf.Sin(phi);
                                float z = r * Mathf.Cos(phi);

                                return new Vector3(x, y, z) + o;
                        }


                }

                public enum AnchorType {
                        stationary,
                        localplayer,
                }
        }
}

