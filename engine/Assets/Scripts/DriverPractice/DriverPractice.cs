using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace Synthesis.DriverPractice
{
    public class DriverPractice
    {
        public string intakeNode;
        public string releaseNode;
        public Vector3 releasePosition;
        public Vector3 releaseVelocity;
        public string gamepiece;
        public DriverPractice(XElement e)
        {
            this.gamepiece = e.Element("Gamepiece").Attribute("id").Value;
            this.intakeNode = e.Element("IntakeNode").Attribute("id").Value;
            this.releaseNode = e.Element("ReleaseNode").Attribute("id").Value;
            XElement rp = e.Element("ReleasePosition");
            this.releasePosition = new Vector3(float.Parse(rp.Attribute("x").Value), float.Parse(rp.Attribute("y").Value), float.Parse(rp.Attribute("z").Value));
            XElement rv = e.Element("ReleaseVelocity");
            this.releaseVelocity = new Vector3(float.Parse(rv.Attribute("x").Value), float.Parse(rv.Attribute("y").Value), float.Parse(rv.Attribute("z").Value));
        }
        public DriverPractice(string gamepiece, string intakeNode, string releaseNode, Vector3 releasePosition, Vector3 releaseVelocity)
        {
            this.gamepiece = gamepiece;
            this.intakeNode = intakeNode;
            this.releaseNode = releaseNode;
            this.releasePosition = releasePosition;
            this.releaseVelocity = releaseVelocity;
        }
    }
}
