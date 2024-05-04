using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using BepInEx;
using HarmonyLib;
using HumanAPI;
using UnityEngine;

namespace spsp
{
        [BepInPlugin("com.kirisoup.hff.spsp", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
        [BepInProcess("Human.exe")]
        public partial class Plugin : BaseUnityPlugin {

                readonly Harmony harmony = new("com.kirisoup.hff.spsp");

                public static bool isEnabled;

                // public void Awake() => harmony.PatchAll();

                // public void OnDestroy() => harmony.UnpatchSelf();

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

                        DrawSphereSamples();
                }

                static void DrawSphereSamples() {

                        if (SphereSamples.Count == 0) return;

                        List<Collider> objs = new();
                        List<Vector3> points = new();

                        foreach (var sample in SphereSamples) foreach (var hitgroup in sample.HitGroups) {

                                foreach (var hit in hitgroup.Key) {

                                        Vector2 pos = Camera.main.WorldToScreenPoint(hit.Pos);

                                        Rect rect = new(pos.x, - pos.y + Screen.height, 100, 100);

                                        Color color = hitgroup.Value switch {
                                                ColType.standard => Color.white,
                                                ColType.rigidbody => Color.cyan,
                                                ColType.invisible => Color.blue,
                                                ColType.trigger => Color.magenta,
                                                ColType.checkpoint => new(1, 0.66f, 0),
                                                ColType.passtrigger => Color.green,
                                                ColType.falltrigger => Color.red,
                                                _ => Color.white,
                                        };

                                        GUIStyle style = new();

                                        style.normal.textColor = color;

                                        GUI.Label(rect, "+", style);
                                }

                        }
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

                        Vector3 origin;
                        Vector3[] ends;
                        EspHit[] hits;
                        EspLine[] lines;
                        Dictionary<EspHit[], ColType> hitGroups;

                        public float Radius { get; set; } = 7.5f;
                        public int Count { get; set; } = 8;

                        public bool SampleInvisible { get; set; } = true;
                        public bool SampleRigid { get; set; } = true;
                        public bool SampleMiscColli { get; set; } = true;
                        public bool SampleCP { get; set; } = true;
                        public bool SamplePTrig { get; set; } = true;
                        public bool SampleFTrig { get; set; } = true;
                        public bool SampleMiscTrig { get; set; } = true;

                        public AnchorType Type { get; set; } = AnchorType.localplayer;

                        Vector3 PlayerPos { get => Human.Localplayer.transform.position + Vector3.up; }

                        Vector3 syncOrigin = new();

                        bool shouldSync { get => Origin == syncOrigin; }

                        public Vector3 Origin {
                                get => Type switch {
                                        AnchorType.stationary => origin,
                                        AnchorType.localplayer => origin = (Vector3.Distance(PlayerPos, syncOrigin) < 1) ? PlayerPos : syncOrigin = PlayerPos,
                                        _ => new(),
                                };
                                set => origin = value;
                        }

                        Vector3[] Ends {
                                // get => Type switch {
                                //         AnchorType.stationary => ends ??= GenEnds(),
                                //         AnchorType.localplayer => ends = shouldSync ? GenEnds() : ends ?? GenEnds(),
                                //         _ => new Vector3[0],
                                // };
                                get => Type switch {
                                        AnchorType.stationary => ends ??= GenEnds(),
                                        AnchorType.localplayer => ends = GenEnds(),
                                        _ => new Vector3[0],
                                };
                        }


                        EspHit[] Hits {
                                // get => Type switch {
                                //         AnchorType.stationary => hits ??= GenHits(),
                                //         AnchorType.localplayer => hits = shouldSync ? GenHits() : hits ?? GenHits(),
                                //         _ => new EspHit[0],
                                // };
                                get => Type switch {
                                        AnchorType.stationary => hits ??= GenHits(),
                                        AnchorType.localplayer => hits = GenHits(),
                                        _ => new EspHit[0],
                                };
                        }

                        public Dictionary<EspHit[], ColType> HitGroups {
                                get => Type switch {
                                        AnchorType.stationary => hitGroups ??= GroupHits(),
                                        AnchorType.localplayer => hitGroups = shouldSync ? GroupHits() : hitGroups ?? GroupHits(),
                                        _ => new(),
                                };
                        }

                        Vector3[] GenEnds() {

                                List<Vector3> points = new();

                                for (int i = 0; i < Count; i++) {

                                        Vector3 p = DistriPointOnSphere(Origin, Radius, i, Count);

                                        points.Add(p);
                                }

                                return points.ToArray();
                        }


                        EspHit[] GenHits() {

                                List<EspHit> hitlist = new();

                                foreach (var end in Ends) {

                                        foreach (var hit in Physics.RaycastAll(Origin, end - Origin, Radius)) {

                                                if (hit.collider.transform.IsChildOf(Human.Localplayer.transform)) continue;

                                                if (Camera.main.WorldToScreenPoint(hit.point).z < 0) continue;

                                                hitlist.Add(new EspHit() { Pos = hit.point, Obj = hit.collider.gameObject });
                                        }
                                }

                                return hitlist.ToArray();
                        }

                        Dictionary<EspHit[], ColType> GroupHits() {

                                Dictionary<GameObject,List<EspHit>> dict = new();
                                Dictionary<EspHit[], ColType> groups = new();

                                foreach (var hit in Hits) {

                                        if (dict.ContainsKey(hit.Obj)) dict[hit.Obj].Add(hit);

                                        else dict.Add(hit.Obj, new() { hit });
                                }

                                foreach (var item in dict) {

                                        ColType type;

                                        if (!item.Key.GetComponent<Collider>().isTrigger) {
                                                if (item.Key.gameObject.GetComponent<Rigidbody>() is not null) type = ColType.rigidbody;
                                                else if (item.Key.gameObject.GetComponent<Renderer>() is not null) type = ColType.standard;
                                                else type = ColType.invisible;
                                        } else {
                                                if (item.Key.gameObject.GetComponent<Checkpoint>() is not null) type = ColType.checkpoint;
                                                else if (item.Key.gameObject.GetComponent<LevelPassTrigger>() is not null) type = ColType.passtrigger;
                                                else if (item.Key.gameObject.GetComponent<FallTrigger>() is not null) type = ColType.falltrigger;
                                                else type = ColType.trigger;
                                        }

                                        if (type!=ColType.standard) continue;

                                        // NOTE: plcc said this approach might cause issue :|
                                        // else color = hit.collider.gameObject.GetComponent<MonoBehaviour>() switch {
                                        //         Checkpoint => new Color(1, 0.66f, 0),
                                        //         LevelPassTrigger => Color.green,
                                        //         FallTrigger => Color.red,
                                        //         _ => Color.magenta,
                                        // };

                                        groups.Add(item.Value.ToArray(), type);
                                }

                                return groups;
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

                public struct EspHit {

                        public Vector3 Pos;
                        public GameObject Obj;
                }

                public struct EspLine {

                        Vector3 p1;
                        Vector3 p2;
                        ColType color;
                }

                public enum AnchorType {
                        stationary,
                        localplayer,
                }

                public enum ColType {
                        standard,
                        rigidbody,
                        invisible,
                        trigger,
                        checkpoint,
                        passtrigger,
                        falltrigger,
                }
        }
}

