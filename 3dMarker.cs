using GTA; 
using GTA.Native; 
using GTA.Math;
using NativeUI;
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

        struct Marker
        {
            public string scaleform_name;
            public float distance;
        };

        void OnTick(object sender, EventArgs e)
        {
            config = ScriptSettings.Load("scripts\\3dMarker.ini");
            int index = 2;
            Blip[] blips = World.GetActiveBlips();

            //there are a finite number of markers avail
            //dequeue until none left
            Queue<string> scaleform_queue = CreateScaleformQueue();

            //keep track of current markers
            //replace if found closer marker
            Dictionary<string, Marker> current_markers = new Dictionary<string, Marker>();

            foreach (Blip b in blips)
            {
                string marker_name = b.Sprite.ToString().ToUpper();
                string enable_disable = config.GetValue<string>("options", marker_name, "disabled");
                if (enable_disable == "enabled")
                {
                    float distance = World.GetDistance(b.Position, GameplayCamera.Position);

                    Vector3 camrot = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_ROT, 0);
                    Vector3 label_pos = CreateLabelCoordinates(distance);
                    Vector3 label_scale = CreateLabelScale(distance);

                    if (scaleform_queue.Count > 0)
                    {
                        string scaleform_name = scaleform_queue.Dequeue();

                        if (current_markers.ContainsKey(marker_name))
                        {
                            Marker tmp_marker = current_markers[marker_name];
                            if(tmp_marker.distance > distance)
                            {
                                
                                Marker new_marker;
                                new_marker.distance = distance;
                                new_marker.scaleform_name = tmp_marker.scaleform_name;
                                current_markers[marker_name] = new_marker;

                                scaleform_queue.Enqueue(scaleform_name);

                                var tmpSFs = new Scaleform(tmp_marker.scaleform_name);
                                tmpSFs.CallFunction("SET_PLAYER_NAME", b.Sprite.ToString());
                                tmpSFs.Render3D(b.Position + label_pos, new Vector3(0f, (0 - camrot.Z), 0), label_scale);
                            }
                        }
                        else
                        {
                            Marker new_marker;
                            new_marker.distance = distance;
                            new_marker.scaleform_name = scaleform_name;
                            current_markers.Add(marker_name, new_marker);

                            var tmpSFs = new Scaleform(scaleform_name);
                            tmpSFs.CallFunction("SET_PLAYER_NAME", b.Sprite.ToString());
                            tmpSFs.Render3D(b.Position + label_pos, new Vector3(0f, (0 - camrot.Z), 0), label_scale);
                        }

                    }
                    else if(! current_markers.ContainsKey(marker_name))
                    {
                        UI.Notify("Maximum of 15 markers can be displayed, cannot display more");
                    }
                }
            }
            //testing
            /*
            foreach (Blip b in blips)
            {
                if(b.Sprite.ToString() == "Michael")
                {
                    float distance = World.GetDistance(b.Position, GameplayCamera.Position);

                    Vector3 camrot = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_ROT, 0);
                    Vector3 label_pos = CreateLabelCoordinates(distance);
                    Vector3 label_scale = CreateLabelScale(distance);

                    //var tmpSFs = new Scaleform("PLAYER_NAME_10");
                    //tmpSFs.CallFunction("SET_PLAYER_NAME", b.Sprite.ToString());
                    //tmpSFs.Render3D(b.Position + label_pos, new Vector3(0f, (0 - camrot.Z), 0), label_scale);
                    var tmpSFs = new Scaleform("ORGANISATION_NAME");
                    tmpSFs.CallFunction("SET_ORGANISATION_NAME", b.Sprite.ToString(), 1, 1, 1);
                    tmpSFs.Render3D(b.Position + label_pos, new Vector3(0f, (0 - camrot.Z), 0), label_scale);
                }
            }
            */
        }

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

        Vector3 CreateLabelCoordinates(float distance)
        {
            float min_height = 10f;
            float max_height = 1500f;
            float height = distance * 0.5f;
            if (height < min_height) height = min_height;
            if (height > max_height) height = max_height;
            return new Vector3(0, 0, height);
        }

        Vector3 CreateLabelScale(float distance)
        {
            float max_distance_modifier = 1000f;
            if (distance > max_distance_modifier) distance = max_distance_modifier;
            return new Vector3(5, 3, 1) * distance * 0.1f;
        }

        void DisplayHelpTextThisFrame(string text)
        {
            Function.Call(Hash._SET_TEXT_COMPONENT_FORMAT, "STRING");
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, text);
            Function.Call(Hash._0x238FFE5C7B0498A6, 0, 0, 1, -1);
        }

    }
}