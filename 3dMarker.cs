using GTA; 
using GTA.Native; 
using GTA.Math;
using System; 
using System.Windows.Forms; 
using System.Drawing;
using System.IO;
using System.Collections.Generic;

namespace Markers3D
{
    public class MainScript : Script
    {
        ScriptSettings config;
        public MainScript()
        {
            config = ScriptSettings.Load("scripts\\3dMarker.ini");

            Tick += OnTick;
            Interval = 1;
        }

        // maintains state about what we have rendered in the world
        struct Marker
        {
            public string scaleform_name;
            public float distance;
        };

        void OnTick(object sender, EventArgs e)
        {
            config = ScriptSettings.Load("scripts\\3dMarker.ini");
            Blip[] blips = World.GetActiveBlips();

            // there are a finite number of scaleforms avail
            // queue them all up
            // if we use one, dequeue
            Queue<string> scaleform_queue = CreateScaleformQueue();

            // keep track of current markers already in world
            // replace if marker already exists but new one is closer
            Dictionary<string, Marker> current_markers = new Dictionary<string, Marker>();

            foreach (Blip b in blips)
            {
                // get blip name
                string marker_name = b.Sprite.ToString().ToUpper();

                // read ini file
                string enable_disable = config.GetValue<string>("options", marker_name, "disabled");

                // begin spaghetti code :(
                // if blip is "enabled" in ini file 
                if (enable_disable == "enabled")
                {

                    // distance from player to blip
                    float distance = World.GetDistance(b.Position, GameplayCamera.Position);

                    // create coordinates, scale, and rotation for scaleform
                    // height and scale based on distance from player
                    Vector3 camrot = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_ROT, 0);
                    Vector3 label_pos = CreateLabelCoordinates(distance);
                    Vector3 label_scale = CreateLabelScale(distance);

                    // if there are still scaleforms available
                    if (scaleform_queue.Count > 0)
                    {
                        string scaleform_name = scaleform_queue.Dequeue();

                        // if blip has already been rendered in game world this tick
                        // this is for duplicates like Ammunation, Clothes, etc
                        if (current_markers.ContainsKey(marker_name))
                        {
                            Marker tmp_marker = current_markers[marker_name];

                            // if proposed blip is closer than blip that's already been placed in world
                            // replace it in world, replace it in current_markers
                            if (tmp_marker.distance > distance)
                            {
                                
                                Marker new_marker;
                                new_marker.distance = distance;
                                new_marker.scaleform_name = tmp_marker.scaleform_name;
                                current_markers[marker_name] = new_marker;

                                var tmpSFs = new Scaleform(tmp_marker.scaleform_name);
                                tmpSFs.CallFunction("SET_PLAYER_NAME", b.Sprite.ToString());
                                tmpSFs.Render3D(b.Position + label_pos, new Vector3(0f, (0 - camrot.Z), 0), label_scale);
                            }

                            // requeue scaleform because we aren't using it
                            scaleform_queue.Enqueue(scaleform_name);
                        }
                        else
                        {
                            // if this is a new blip, add it to world and current_markers list
                            Marker new_marker;
                            new_marker.distance = distance;
                            new_marker.scaleform_name = scaleform_name;
                            current_markers.Add(marker_name, new_marker);

                            var tmpSFs = new Scaleform(scaleform_name);
                            tmpSFs.CallFunction("SET_PLAYER_NAME", b.Sprite.ToString());
                            tmpSFs.Render3D(b.Position + label_pos, new Vector3(0f, (0 - camrot.Z), 0), label_scale);
                        }
                    }
                }
            }
        }

        // finite amount of scaleforms to use
        Queue<string> CreateScaleformQueue()
        {
            Queue<string> my_queue = new Queue<string>();
            my_queue.Enqueue("PLAYER_NAME_01");
            my_queue.Enqueue("PLAYER_NAME_02");
            my_queue.Enqueue("PLAYER_NAME_03");
            my_queue.Enqueue("PLAYER_NAME_04");
            my_queue.Enqueue("PLAYER_NAME_05");
            my_queue.Enqueue("PLAYER_NAME_06");
            my_queue.Enqueue("PLAYER_NAME_07");
            my_queue.Enqueue("PLAYER_NAME_08");
            my_queue.Enqueue("PLAYER_NAME_09");
            my_queue.Enqueue("PLAYER_NAME_10");
            my_queue.Enqueue("PLAYER_NAME_11");
            my_queue.Enqueue("PLAYER_NAME_12");
            my_queue.Enqueue("PLAYER_NAME_13");
            my_queue.Enqueue("PLAYER_NAME_14");
            my_queue.Enqueue("PLAYER_NAME_15");
            return my_queue;
        }

        // clamp scaleform height between predefined min, max
        Vector3 CreateLabelCoordinates(float distance)
        {
            float min_height = 10f;
            float max_height = 1500f;
            float height = distance * 0.5f;
            if (height < min_height) height = min_height;
            if (height > max_height) height = max_height;
            return new Vector3(0, 0, height);
        }

        // clamp scaleform scale between predefined min, max
        Vector3 CreateLabelScale(float distance)
        {
            float max_distance_modifier = 1000f;
            if (distance > max_distance_modifier) distance = max_distance_modifier;
            return new Vector3(5, 3, 1) * distance * 0.1f;
        }

    }
}