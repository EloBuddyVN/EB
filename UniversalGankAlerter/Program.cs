using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EloBuddy;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Rendering;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using System.Collections;
using EloBuddy.SDK.Enumerations;
using System.Drawing;

namespace UniversalGankAlerter
{
    class Program
    {
        private static Program _instance;
         
        private readonly IDictionary<int, ChampionInfo> _championInfoById = new Dictionary<int, ChampionInfo>();
        private Menu _menu;
        private Slider _sliderRadius;
        private PreviewCircle _previewCircle;
        private Slider _sliderCooldown;
        private Slider _sliderLineDuration;
        private CheckBox _enemyJunglerOnly;
        private CheckBox _allyJunglerOnly;
        private CheckBox _showChampionNames;
        private CheckBox _drawMinimapLines;
        private CheckBox _dangerPing;
        private Menu _enemies;
        private Menu _allies;

        public int Radius
        {
            get { return _sliderRadius.CurrentValue; }
        }

        public int Cooldown
        {
            get { return _sliderCooldown.CurrentValue; }
        }

        public bool DangerPing
        {
            get { return _dangerPing.CurrentValue; }
        }

        public int LineDuration
        {
            get { return _sliderLineDuration.CurrentValue; }
        }

        public bool EnemyJunglerOnly
        {
            get { return _enemyJunglerOnly.CurrentValue; }
        }

        public bool AllyJunglerOnly
        {
            get { return _allyJunglerOnly.CurrentValue; }
        }

        public bool ShowChampionNames
        {
            get { return _showChampionNames.CurrentValue; }
        }

        public bool DrawMinimapLines
        {
            get { return _drawMinimapLines.CurrentValue; }
        }

        private static void Main(string[] args)
        {
            _instance = new Program();
        }

        public static Program Instance()
        {
            return _instance;
        }

        private Program()
        {
            Loading.OnLoadingComplete += Game_OnGameLoad;
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            _previewCircle = new PreviewCircle();

            _menu = MainMenu.AddMenu("Universal GankAlerter", "universalgankalerter");
            _sliderRadius = new Slider("Trigger range", 3000, 500, 5000);
            _sliderRadius.OnValueChange += SliderRadiusValueChanged;
            _sliderCooldown = new Slider("Trigger cooldown (sec)", 10, 0, 60);
            _sliderLineDuration = new Slider("Line duration (sec)", 10, 0, 20);
            _enemyJunglerOnly = new CheckBox("Warn enemy jungler only (smite)", false);
            _allyJunglerOnly = new CheckBox("Warn ally jungler only (smite)", true);
            _showChampionNames = new CheckBox("Show champion name", true);
            _drawMinimapLines = new CheckBox("Draw minimap lines", false);
            _dangerPing = new CheckBox("Danger Ping (local)", false);
            _enemies = _menu.AddSubMenu("Enemies", "enemies");
            _enemies.Add<CheckBox>("enemyjungleronly", _enemyJunglerOnly);

            _allies = _menu.AddSubMenu("Allies", "allies");
            _allies.Add<CheckBox>("allyjungleronly",_allyJunglerOnly);

            _menu.Add<Slider>("radius",_sliderRadius);
            _menu.Add<Slider>("cooldown", _sliderCooldown);
            _menu.Add<Slider>("lineduration", _sliderLineDuration);
            _menu.Add<CheckBox>("shownames", _showChampionNames);
            _menu.Add<CheckBox>("drawminimaplines", _drawMinimapLines);
            _menu.Add<CheckBox>("dangerping", _dangerPing);
            //_menu.AddSubMenu(_enemies);
            //_menu.AddSubMenu(_allies);
            foreach (AIHeroClient hero in ObjectManager.Get<AIHeroClient>())
            {
                if (hero.NetworkId != ObjectManager.Player.NetworkId)
                {
                    if (hero.IsEnemy)
                    {
                        _championInfoById[hero.NetworkId] = new ChampionInfo(hero, false);
                        _enemies.Add<CheckBox>("enemy" + hero.ChampionName,new CheckBox(hero.ChampionName));
                    }
                    else
                    {
                        _championInfoById[hero.NetworkId] = new ChampionInfo(hero, true);
                        _allies.Add<CheckBox>("ally" + hero.ChampionName, new CheckBox(hero.ChampionName,false));
                    }
                }
            }
            //_menu.AddToMainMenu();
            Print("Loaded!");
        }

        private void SliderRadiusValueChanged(object sender, Slider.ValueChangeArgs e)
        {
            _previewCircle.SetRadius(e.NewValue);
        }

        private static void Print(string msg)
        {
            Chat.Print(
                "<font color='#ff3232'>Universal</font><font color='#d4d4d4'>GankAlerter:</font> <font color='#FFFFFF'>" +
                msg + "</font>");
        }

        public bool IsEnabled(AIHeroClient hero)
        {
            return hero.IsEnemy
                ? _enemies["enemy" + hero.ChampionName].Cast<CheckBox>().CurrentValue
                : _allies["ally" + hero.ChampionName].Cast<CheckBox>().CurrentValue;
        }
    }

    internal class PreviewCircle
    {
        private const int Delay = 2;

        private float _lastChanged;
        private readonly Circle _mapCircle;
        private int _radius;

        public PreviewCircle()
        {
            Drawing.OnEndScene += Drawing_OnEndScene;
            //_mapCircle = new Circle(ObjectManager.Player, 0, System.Drawing.Color.Red, 5);
            //_mapCircle.Add(0);
            //_mapCircle.VisibleCondition = sender => _lastChanged > 0 && Game.Time - _lastChanged < Delay;
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
           //Chat.Print("before if " + _lastChanged);
            if (_lastChanged > 0 && Game.Time - _lastChanged < Delay)
            {
                Drawing.DrawCircle(Player.Instance.Position, _radius, System.Drawing.Color.Red);
                //Drawing.DrawCircle(Drawing.WorldToMinimap(Player.Instance.Position).To3D(), 500, System.Drawing.Color.Red);
                //Drawing.DrawText(Drawing.WorldToMinimap(Player.Instance.Position),Color.Red, "Ermagherd", 30);
            }
        }

        public void SetRadius(int radius)
        {
            _radius = radius;
            //_mapCircle.Radius = (float) radius;
            _lastChanged = Game.Time;
        }
    }

    internal class ChampionInfo
    {
        private static int index = 0;

        private readonly AIHeroClient _hero;
        private readonly bool _ally;
        private int textoffset;

        private event EventHandler OnEnterRange;

        private bool _visible;
        private float _distance;
        private float _lastEnter;
        private float _lineStart;
        private int _lineWidth;

        public ChampionInfo(AIHeroClient hero, bool ally)
        {
            index++;
            textoffset = index * 50;
            _hero = hero;
            _ally = ally;
          /*  Text text = new Text(_hero.ChampionName, 20,
                ally
                    ? Color.FromArgb(255, 205, 255, 205)
                    : Color.FromArgb(255, 255, 205, 205)
            {
                PositionUpdate =
                    () =>
                        Drawing.WorldToScreen(
                            Player.Instance.ServerPosition.to2D().extend(_hero.Position, 300 + textoffset)),
                VisibleCondition = delegate
                {
                    float dist = _hero.Distance(ObjectManager.Player.Position);
                    return Program.Instance().ShowChampionNames && !_hero.IsDead &&
                           Game.Time - _lineStart < Program.Instance().LineDuration &&
                           (!_hero.IsVisible || !Render.OnScreen(Drawing.WorldToScreen(_hero.Position))) &&
                           dist < Program.Instance().Radius && dist > 300 + textoffset;
                },
                Centered = true,
                OutLined = true,
            };*/
            

           // text.Add(1);
      /*      _line = new Line(
                new Vector2(), new Vector2(), 5,
                ally ? new Color { R = 0, G = 255, B = 0, A = 125 } : new Color { R = 255, G = 0, B = 0, A = 125 })
            {
                StartPositionUpdate = () => Drawing.WorldToScreen(ObjectManager.Player.Position),
                EndPositionUpdate = () => Drawing.WorldToScreen(_hero.Position),
                VisibleCondition =
                    delegate
                    {
                        return !_hero.IsDead && Game.Time - _lineStart < Program.Instance().LineDuration &&
                               _hero.Distance(ObjectManager.Player.Position) < (Program.Instance().Radius + 1000);
                    }
            };*/

           // _line.Add(0);
     /*       Line minimapLine = new Line(
                new Vector2(), new Vector2(), 2,
                ally ? new Color { R = 0, G = 255, B = 0, A = 255 } : new Color { R = 255, G = 0, B = 0, A = 255 })
            {
                StartPositionUpdate = () => Drawing.WorldToMinimap(ObjectManager.Player.Position),
                EndPositionUpdate = () => Drawing.WorldToMinimap(_hero.Position),
                VisibleCondition =
                    delegate
                    {
                        return Program.Instance().DrawMinimapLines && !_hero.IsDead && Game.Time - _lineStart < Program.Instance().LineDuration;
                    }
            };*/



           // minimapLine.Add(0);
            Game.OnUpdate += Game_OnGameUpdate;
            OnEnterRange += ChampionInfo_OnEnterRange;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (Program.Instance().DrawMinimapLines && !_hero.IsDead && Game.Time - _lineStart < Program.Instance().LineDuration)
                Drawing.DrawLine(Drawing.WorldToMinimap(ObjectManager.Player.Position), Drawing.WorldToMinimap(_hero.Position), 2, _ally ? Color.FromArgb(125, 0, 255, 0) : Color.FromArgb(125, 255, 0, 0));
        }

        private void OnDraw(EventArgs args)
        {
            //Chat.Print("gt here" + _hero.Name);
            try
            {
                string txt = _hero.ChampionName;
                Color color = _ally
                        ? Color.FromArgb(255, 205, 255, 205)
                        : Color.FromArgb(255, 255, 205, 205);
                float dist = _hero.Distance(ObjectManager.Player.Position);
                if (Program.Instance().ShowChampionNames && !_hero.IsDead &&
                               Game.Time - _lineStart < Program.Instance().LineDuration &&
                              // !_hero.IsVisible &&
                               dist < Program.Instance().Radius && dist > 300 + textoffset)
                    Drawing.DrawText(Drawing.WorldToScreen(Player.Instance.ServerPosition.To2D().Extend(_hero, 300f + textoffset).To3D()), color, txt, 40);

                if (!_hero.IsDead && Game.Time - _lineStart < Program.Instance().LineDuration &&
                       _hero.Distance(ObjectManager.Player.Position) < (Program.Instance().Radius + 1000))
                    Drawing.DrawLine(Drawing.WorldToScreen(Player.Instance.Position), Drawing.WorldToScreen(_hero.Position), _lineWidth, _ally ? Color.FromArgb(125, 0, 255, 0) : Color.FromArgb(125, 255, 0, 0));
            } catch (Exception e)
            {
                Chat.Print("Exception");
                Console.WriteLine(e.StackTrace);
            }


            Color c = _ally ? Color.FromArgb(125, 0, 255, 0) : Color.FromArgb(125, 255, 0, 0);
            if (!_hero.IsDead && Game.Time - _lineStart < Program.Instance().LineDuration &&
                               _hero.Distance(ObjectManager.Player.Position) < (Program.Instance().Radius + 1000))
                Drawing.DrawLine(Player.Instance.Position.To2D(), _hero.Position.To2D(), 5, c);

            //Drawing.DrawCircle(Player.Instance.Position, 200, Color.Red);
        }

        private void ChampionInfo_OnEnterRange(object sender, EventArgs e)
        {
            bool enabled = false;
            if (Program.Instance().EnemyJunglerOnly && _hero.IsEnemy)
            {
                enabled = IsJungler(_hero);
            }
            else if (Program.Instance().AllyJunglerOnly && _hero.IsAlly)
            {
                enabled = IsJungler(_hero);
            }
            else
            {
                enabled = Program.Instance().IsEnabled(_hero);
            }

            if (Game.Time - _lastEnter > Program.Instance().Cooldown && enabled)
            {
                _lineStart = Game.Time;
                if (Program.Instance().DangerPing && _hero.IsEnemy && !_hero.IsDead)
                {
                    TacticalMap.ShowPing(PingCategory.Danger, _hero, true);
                }
            }
            _lastEnter = Game.Time;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            float newDistance = _hero.Distance(ObjectManager.Player);

            if (Game.Time - _lineStart < Program.Instance().LineDuration)
            {
                float percentage = newDistance / Program.Instance().Radius;
                if (percentage <= 1)
                {
                    _lineWidth = (int)(2 + (percentage * 8));
                }
            }

            if (newDistance < Program.Instance().Radius && _hero.IsVisible)
            {
                if (_distance >= Program.Instance().Radius || !_visible)
                {
                    if (OnEnterRange != null)
                    {
                        OnEnterRange(this, null);
                    }
                }
                else if (_distance < Program.Instance().Radius && _visible)
                {
                    _lastEnter = Game.Time;
                }
            }
            _distance = newDistance;
            _visible = _hero.IsVisible;
        }

        private bool IsJungler(AIHeroClient hero)
        {
            return hero.Spellbook.Spells.Any(spell => spell.Name.ToLower().Contains("smite"));
        }
    }
}
