using UnityEngine;
using Weapons;

public class LoadOuts : MonoBehaviour
{

}

public abstract class Loadouts
{
    public Weapon Primary { get; set; }
    public Weapon Secondary { get; set; }
    public Weapon Tertiary { get; set; }

    public class A : Loadouts
    {
        public A()
        {
            Primary = new Weapon.SMG();
            Secondary = new Weapon.Pistol();
            Tertiary = new Weapon.BAM16A4();
            Tertiary.Attachment = "Starburst";
        }
    }

    public class B : Loadouts
    {
        public B()
        {
            Primary = new Weapon.BoltActionSniper();
            Secondary = new Weapon.BAM16A4();
            Secondary.Attachment = "Extended Mags";
            Tertiary = new Weapon.SMG();
        }

    }

    public class C : Loadouts
    {
        public C()
        {
            Primary = new Weapon.Pistol();
            Secondary = new Weapon.Sniper();
            Tertiary = new Weapon.BoltActionSniper();
        }

    }

}
