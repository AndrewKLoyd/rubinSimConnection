using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Connection.ConnTypes;
using System;

public class Addresses
{

    private static Addresses addresses = new Addresses();

    public static readonly string pathToJSONFile = Directory.GetCurrentDirectory() + "/connConfig.json";

    public Dictionary<Pub, string> pubAddr { get; set; } = new Dictionary<Pub, string>();
    public Dictionary<Sub, string> subAddr { get; set; } = new Dictionary<Sub, string>();

    private bool IsEmpty()
    {
        if (pubAddr.Keys.Count == 0 || subAddr.Keys.Count == 0)
            return true;
        return false;
    }

    public static string GetPubIp(Pub pub)
    {
        if (addresses.IsEmpty()) LoadFromJSON();
        return addresses.pubAddr[pub];
    }

    public static void SetSubIp(Sub sub, string ip)
    {
        addresses.subAddr[sub] = ip;
        string json = JsonConvert.SerializeObject(addresses);
        File.WriteAllTextAsync(pathToJSONFile, json);
    }
    public static string GetSubIp(Sub sub)
    {
        if (addresses.IsEmpty()) LoadFromJSON();



        return addresses.subAddr[sub];
    }

    private static void LoadFromJSON()
    {
        if (!File.Exists(pathToJSONFile))
        {
            GenerateDefaultJSON();
            return;
        }

        try
        {
            addresses = JsonConvert.DeserializeObject<Addresses>(File.ReadAllText(pathToJSONFile));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error parsing/retrieving JSON\nFull error: {e.Message}");
        }

    }

    private static void GenerateDefaultJSON()
    {
        //TODO: Modify when new msg was added
        #region Publishers
        addresses.pubAddr.Add(Pub.ANPA, "tcp://*:8080");
        addresses.pubAddr.Add(Pub.ANPAGroup, "tcp://*:8081");
        addresses.pubAddr.Add(Pub.ANPAGroupTab, "tcp://*:8089");
        addresses.pubAddr.Add(Pub.SimEvent, "tcp://*:8082");
        addresses.pubAddr.Add(Pub.CustomSim, "tcp://*:8090");
        addresses.pubAddr.Add(Pub.SimBoundingBoxes, "tcp://*:8083");
        addresses.pubAddr.Add(Pub.IsInKTS, "tcp://*:8084");
        #endregion

        #region Subscrbers

        #region TabSubscribers
        addresses.subAddr.Add(Sub.SimInit, "tcp://localhost:8101");
        addresses.subAddr.Add(Sub.Mission, "tcp://localhost:8102");
        addresses.subAddr.Add(Sub.Custom, "tcp://localhost:8110");
        #endregion

        #region  RegulatorSubscribers
        addresses.subAddr.Add(Sub.RegulatorComplex, "tcp://localhost:8092");
        addresses.subAddr.Add(Sub.GroupTrajectory, "tcp://localhost:8095");
        addresses.subAddr.Add(Sub.MathModelSwitch, "tcp://localhost:8096");
        addresses.subAddr.Add(Sub.CustomSGRU, "tcp://localhost:8100");
        addresses.subAddr.Add(Sub.CustomSGRUEvent, "tcp://localhost:8111");
        #endregion

        #endregion
        string json = JsonConvert.SerializeObject(addresses);
        File.WriteAllTextAsync(pathToJSONFile, json);
    }
}