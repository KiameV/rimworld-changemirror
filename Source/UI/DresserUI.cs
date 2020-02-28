﻿using ChangeMirror.UI.DTO;
using ChangeMirror.UI.Util;
using RimWorld;
using UnityEngine;
using Verse;
using ChangeMirror.UI.Enums;
using ChangeMirror.UI.DTO.SelectionWidgetDTOs;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace ChangeMirror.UI
{
    internal class DresserUI : Window
    {
        private static readonly long TICKS_PER_YEAR = 3600000L;
        private static readonly long MAX_AGE = 1000000000 * TICKS_PER_YEAR;
        private DresserDTO dresserDto;

        private bool rerenderPawn = false;

        private bool saveChangedOnExit = false;

        private bool originalHatsHideSetting;

        private List<Apparel> ApparelWithColorChange = new List<Apparel>();

        public DresserUI(DresserDTO dresserDto)
        {
            this.doCloseButton = false;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.dresserDto = dresserDto;
            this.dresserDto.SetUpdatePawnListeners(this.UpdatePawn);
            this.dresserDto.EditorTypeSelectionDto.SelectionChangeListener += delegate (object sender)
            {
                this.rerenderPawn = true;
            };
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(650f, 600f);
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();
            this.originalHatsHideSetting = Prefs.HatsOnlyOnMap;
        }

        public override void PostClose()
        {
            base.PostClose();
            Prefs.HatsOnlyOnMap = this.originalHatsHideSetting;
            this.dresserDto.Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            PortraitsCache.SetDirty(this.dresserDto.Pawn);
        }
        
        public override void DoWindowContents(Rect inRect)
        {
            try
            {
                if (this.rerenderPawn)
                {
                    this.dresserDto.Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    PortraitsCache.SetDirty(this.dresserDto.Pawn);
                    this.rerenderPawn = false;
                }

                Text.Font = GameFont.Medium;

                Widgets.Label(new Rect(0f, 0f, this.InitialSize.y / 2f + 45f, 50f), "ChangeMirror.MirrorLabel".Translate());

                float portraitBuffer = 30f;

                Rect portraitRect = WidgetUtil.AddPortraitWidget(portraitBuffer, 150f, this.dresserDto);

                float editorLeft = portraitRect.xMax + portraitBuffer;
                float editorTop = 30f + WidgetUtil.SelectionRowHeight;
                float editorWidth = 325f;

                WidgetUtil.AddSelectorWidget(portraitRect.xMax + portraitBuffer, 10f, editorWidth, null, this.dresserDto.EditorTypeSelectionDto);

                switch ((CurrentEditorEnum)this.dresserDto.EditorTypeSelectionDto.SelectedItem)
                {
                    case CurrentEditorEnum.ChangeMirrorApparelColor:
                        WidgetUtil.AddAppararelColorSelectionWidget(editorLeft, editorTop, editorWidth, this.dresserDto.ApparelSelectionsContainer);
                        break;
                    case CurrentEditorEnum.ChangeMirrorBody:
                        bool isShowing = false;
                        float top = editorTop;
                        if (this.dresserDto.BodyTypeSelectionDto != null && this.dresserDto.BodyTypeSelectionDto.Count > 1)
                        {
                            WidgetUtil.AddSelectorWidget(editorLeft, top, editorWidth, "ChangeMirror.BodyType".Translate() + ":", this.dresserDto.BodyTypeSelectionDto);
                            top += WidgetUtil.SelectionRowHeight + 20f;
                            isShowing = true;
                        }
                        if (this.dresserDto.HeadTypeSelectionDto != null && this.dresserDto.HeadTypeSelectionDto.Count > 1)
                        {
                            WidgetUtil.AddSelectorWidget(editorLeft, top, editorWidth, "ChangeMirror.HeadType".Translate() + ":", this.dresserDto.HeadTypeSelectionDto);
                            top += WidgetUtil.SelectionRowHeight + 20f;
                            isShowing = true;
                        }
                        if (this.dresserDto.SkinColorSliderDto != null)
                        {
                            WidgetUtil.AddSliderWidget(editorLeft, top, editorWidth, "ChangeMirror.SkinColor".Translate() + ":", this.dresserDto.SkinColorSliderDto);
                            isShowing = true;
                        }

                        if (!isShowing)
                        {
                            GUI.Label(new Rect(editorLeft, top, editorWidth, 40), "ChangeMirror.NoEditableAttributes".Translate());
                        }

                        if (Settings.ShowGenderAgeChange)
                        {
                            GUI.Label(new Rect(editorLeft, 300f, editorWidth, 40f), "ChangeMirror.GenderChangeWarning".Translate(), WidgetUtil.MiddleCenter);
                            top = 340f;
                            WidgetUtil.AddSelectorWidget(editorLeft, top, editorWidth, "ChangeMirror.Gender".Translate() + ":", this.dresserDto.GenderSelectionDto);

                            top += WidgetUtil.SelectionRowHeight + 5;
                            long ageBio = this.dresserDto.Pawn.ageTracker.AgeBiologicalTicks;
                            if (AddLongInput(editorLeft, top, 120, 80, "ChangeMirror.AgeBiological".Translate() + ":", ref ageBio, MAX_AGE, TICKS_PER_YEAR))
                            {
                                this.dresserDto.Pawn.ageTracker.AgeBiologicalTicks = ageBio;
                                rerenderPawn = true;
                                if (ageBio > this.dresserDto.Pawn.ageTracker.AgeChronologicalTicks)
                                {
                                    this.dresserDto.Pawn.ageTracker.AgeChronologicalTicks = ageBio;
                                }
                            }

                            top += WidgetUtil.SelectionRowHeight + 5;
                            long ageChron = this.dresserDto.Pawn.ageTracker.AgeChronologicalTicks;
                            if (AddLongInput(editorLeft, top, 120, 80, "ChangeMirror.AgeChronological".Translate() + ":", ref ageChron, MAX_AGE, TICKS_PER_YEAR))
                            {
                                this.dresserDto.Pawn.ageTracker.AgeChronologicalTicks = ageChron;
                            }
                        }
                        break;
                    case CurrentEditorEnum.ChangeMirrorHair:
                        const float listboxHeight = 250f;
                        if (this.dresserDto.HairStyleSelectionDto != null)
                        {
                            //bool showHairColor = this.dresserDto.HairColorSelectionDto != null;

                            float height = listboxHeight;
                            /*if (!showHairColor)
                            {
                                height += 250;
                            }*/

                            WidgetUtil.AddListBoxWidget(editorLeft, editorTop, editorWidth, height, "ChangeMirror.HairStyle".Translate() + ":", this.dresserDto.HairStyleSelectionDto);

                            //if (showHairColor)
                            //{
                            WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop + listboxHeight + 10f, editorWidth, this.dresserDto.HairColorSelectionDto, this.dresserDto.HairColorSelectionDto.ColorPresetsDTO);
                            //}
                        }
                        break;


                    case CurrentEditorEnum.ChangeMirrorAlienSkinColor:
                        if (this.dresserDto.AlienSkinColorPrimary != null)
                        {
                            GUI.color = Color.white;
                            Text.Font = GameFont.Medium;
                            GUI.Label(new Rect(editorLeft, editorTop, editorWidth, 30), "ChangeMirror.AlienPrimarySkinColor".Translate());
                            Text.Font = GameFont.Small;

                            WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop + 40, editorWidth, this.dresserDto.AlienSkinColorPrimary, null);
                        }
                        if (this.dresserDto.AlienSkinColorSecondary != null)
                        {
                            GUI.color = Color.white;
                            Text.Font = GameFont.Medium;
                            GUI.Label(new Rect(editorLeft, editorTop + 260, editorWidth, 30), "ChangeMirror.AlienSecondarySkinColor".Translate());
                            Text.Font = GameFont.Small;

                            WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop + 300, editorWidth, this.dresserDto.AlienSkinColorSecondary, null);
                        }
                        break;

                    /*case CurrentEditorEnum.ChangeDresserAlienHairColor:
                        if (this.dresserDto.AlienHairColorPrimary != null)
                        {
                            GUI.color = Color.white;
                            Text.Font = GameFont.Medium;
                            GUI.Label(new Rect(editorLeft, editorTop, editorWidth, 30), "ChangeMirror.AlienPrimaryHairColor".Translate());
                            Text.Font = GameFont.Small;

                            WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop + 40, editorWidth, this.dresserDto.AlienHairColorPrimary, null);
                        }

                        if (this.dresserDto.AlienHairColorSecondary != null)
                        {
                            GUI.color = Color.white;
                            Text.Font = GameFont.Medium;
                            GUI.Label(new Rect(editorLeft, editorTop + 260, editorWidth, 30), "ChangeMirror.AlienSecondaryHairColor".Translate());
                            Text.Font = GameFont.Small;

                            WidgetUtil.AddColorSelectorWidget(editorLeft, editorTop + 300, editorWidth, this.dresserDto.AlienHairColorSecondary, null);
                        }
                        break;*/
                }

                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Small;
                GUI.Label(new Rect(0, 75, this.InitialSize.y / 2f, 50f), GUI.tooltip);
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.UpperLeft;

                float xWidth = 150;
                float xBuffer = (this.InitialSize.x - xWidth) / 2;
                Rect bottomButtonsRect = new Rect(editorLeft, this.InitialSize.y - WidgetUtil.SelectionRowHeight - 36, xWidth, WidgetUtil.SelectionRowHeight);
                GUI.BeginGroup(bottomButtonsRect);
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                if (Widgets.ButtonText(new Rect(0, 0, 60, WidgetUtil.SelectionRowHeight), "Reset".Translate()))
                {
                    this.ResetToDefault();
                    this.ApparelWithColorChange.Clear();
                }
                if (Widgets.ButtonText(new Rect(90, 0, 60, WidgetUtil.SelectionRowHeight), "Save".Translate()))
                {
                    this.saveChangedOnExit = true;
                    this.Close();
                }
                GUI.EndGroup();
            }
            catch (Exception e)
            {
                Log.Error(this.GetType().Name + " closed due to: " + e.GetType().Name + " " + e.Message);
                Messages.Message(this.GetType().Name + " closed due to: " + e.GetType().Name + " " + e.Message, MessageTypeDefOf.NegativeEvent);
                base.Close();
            }
            finally
            {
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }

        private bool AddLongInput(float labelLeft, float top, float inputLeft, float inputWidth, string label, ref long value, long maxValue, long factor = 1)
        {
            string stringValue;
            if (value == -1)
            {
                stringValue = "";
            }
            else
            {
                stringValue = (value / factor).ToString();
            }
            string result = WidgetUtil.AddNumberTextInput(labelLeft, top, inputLeft, inputWidth, label, stringValue);
            try
            {
                if (result.Length == 0)
                {
                    value = -1;
                    return true;
                }
                else if (result.Length > 0 && !result.Equals(stringValue))
                {
                    value = long.Parse(result);
                    if (value < 0)
                    {
                        value = 0;
                    }
                    else
                    {
                        value *= factor;
                        if (value > maxValue || value < 0)
                            value = maxValue;
                    }
                    return true;
                }
            }
            catch { }
            return false;
        }

        private void ResetToDefault()
        {
#if TRACE
            Log.Warning(Environment.NewLine + "Begin DresserUI.ResetToDefault");
#endif

            this.dresserDto.ResetToDefault();
            this.UpdatePawn(null, null);
            
#if TRACE
            Log.Warning("End DresserUI.ResetToDefault" + Environment.NewLine);
#endif
        }

        public override void PreClose()
        {
#if TRACE
            Log.Message(Environment.NewLine + "Start DresserUI.PreClose");
#endif
            try
            {
                base.PreClose();

                if (this.dresserDto != null)
                {
                    if (this.dresserDto.ApparelSelectionsContainer != null &&
                        this.dresserDto.ApparelSelectionsContainer.ColorPresetsDTO != null &&
                        this.dresserDto.ApparelSelectionsContainer.ColorPresetsDTO.IsModified == true)
                    {
                        IOUtil.SaveColorPresets(ColorPresetType.Apparel, this.dresserDto.ApparelSelectionsContainer.ColorPresetsDTO);
                    }

                    if (this.dresserDto.HairColorSelectionDto != null &&
                        this.dresserDto.HairColorSelectionDto.ColorPresetsDTO != null &&
                        this.dresserDto.HairColorSelectionDto.ColorPresetsDTO.IsModified == true)
                    {
                        IOUtil.SaveColorPresets(ColorPresetType.Hair, this.dresserDto.HairColorSelectionDto.ColorPresetsDTO);
                    }
                }

                if (this.ApparelWithColorChange != null)
                {
#if DEBUG
                    Log.Warning(" this.ApparelWithColorChange.Count: " + this.ApparelWithColorChange.Count);
#endif
                    this.ApparelWithColorChange.Clear();
                    this.ApparelWithColorChange = null;
                }

                if (!this.saveChangedOnExit)
                {
                    this.ResetToDefault();
                }
            }
            catch (Exception e)
            {
                Log.Error("Error on DresserUI.PreClose: " + e.GetType().Name + " " + e.Message + Environment.NewLine + e.StackTrace);
            }
#if TRACE
            Log.Message("End DresserUI.PreClose" + Environment.NewLine);
#endif
        }

        private void UpdatePawn(object sender, object value)
        {
            if (sender != null)
            {
                Pawn pawn = this.dresserDto.Pawn;

                if (sender is ApparelColorSelectionDTO)
                {
                    if (this.ApparelWithColorChange != null)
                    {
                        ApparelColorSelectionDTO dto = (ApparelColorSelectionDTO)sender;
                        CompColorableUtility.SetColor(dto.Apparel, dto.SelectedColor, true);

                        if (!this.ApparelWithColorChange.Contains(dto.Apparel))
                        {
                            this.ApparelWithColorChange.Add(dto.Apparel);
                        }
                    }
                }
                if (sender is BodyTypeSelectionDTO)
                {
                    pawn.story.bodyType = (BodyTypeDef)value;
                }
                else if (sender is GenderSelectionDTO)
                {
                    pawn.gender = (Gender)value;
                }
                else if (sender is HairColorSelectionDTO)
                {
                    pawn.story.hairColor = (Color)value;
                }
                else if (sender is HairStyleSelectionDTO)
                {
                    pawn.story.hairDef = (HairDef)value;
                }
                else if (sender is HeadTypeSelectionDTO)
                {
                    if (value.ToString().IndexOf("Narrow") >= 0 || 
                        value.ToString().IndexOf("narrow") >= 0)
                    {
                        dresserDto.Pawn.story.crownType = CrownType.Narrow;
                    }
                    else
                    {
                        dresserDto.Pawn.story.crownType = CrownType.Average;
                    }
                    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dresserDto.Pawn.story, value);
                }
                else if (sender is SliderWidgetDTO)
                {
                    pawn.story.melanin = (float)value;
                }
            }
            rerenderPawn = true;
        }
    }
}
