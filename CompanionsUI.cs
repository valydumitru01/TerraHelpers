using System;
using System.Collections.Generic;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;

namespace Companions
{
    // Checkbox UI element
    public class UICheckbox : UIElement
    {
        private UIText label; // Text label next to the checkbox
        private bool isChecked; // State of the checkbox
        private Texture2D checkboxTexture; // Texture for the checkbox

        // Event triggered when the checkbox state changes
        public event Action<bool> OnCheckedChanged;

        // Constructor to initialize the checkbox
        public UICheckbox(string text, bool defaultState = false)
        {
            isChecked = defaultState;
            checkboxTexture = Main.Assets.Request<Texture2D>("Images/UI/Checkbox").Value; // Using a built-in checkbox texture

            // Create and style the text label
            label = new UIText(text);
            label.Left.Set(24f, 0f); // Position the label to the right of the checkbox
            Append(label);

            // Set the dimensions of the checkbox element
            Width.Set(24f + label.MinWidth.Pixels, 0f);
            Height.Set(24f, 0f);
        }

        // Override the LeftClick method to handle click events
        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);
            Toggle();
        }

        // Method to toggle the checkbox state
        private void Toggle()
        {
            isChecked = !isChecked;
            OnCheckedChanged?.Invoke(isChecked);
        }

        // Method to draw the checkbox
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            // Calculate the position to draw the checkbox
            CalculatedStyle dimensions = GetDimensions();
            Vector2 position = new Vector2(dimensions.X, dimensions.Y);

            // Draw the checkbox background
            spriteBatch.Draw(checkboxTexture, position, Color.White);

            // If checked, draw the checkmark
            if (isChecked)
            {
                Texture2D checkmarkTexture = Main.Assets.Request<Texture2D>("Images/UI/Checkmark").Value;
                spriteBatch.Draw(checkmarkTexture, position, Color.White);
            }
        }
    }
    public class CompanionUI : UIState
    {
        private UIPanel panel;
        private UIList companionList;
        private UIScrollbar scrollbar;
        private bool visible = false;

        // Toggle the visibility of the companion management UI
        public void ToggleUI()
        {
            visible = !visible;
            if (visible)
            {
                Append(panel);
            }
            else
            {
                RemoveChild(panel);
            }
        }

        // Initialize the UI elements
        public override void OnInitialize()
        {
            panel = new UIPanel();
            panel.SetPadding(10);
            panel.Left.Set(400f, 0f);
            panel.Top.Set(200f, 0f);
            panel.Width.Set(300f, 0f);
            panel.Height.Set(400f, 0f);

            companionList = new UIList();
            companionList.Width.Set(-25f, 1f);
            companionList.Height.Set(0f, 1f);
            companionList.ListPadding = 5f;

            scrollbar = new UIScrollbar();
            scrollbar.SetView(100f, 1000f);
            scrollbar.Height.Set(0f, 1f);
            scrollbar.Left.Set(275f, 0f);

            panel.Append(companionList);
            panel.Append(scrollbar);
            companionList.SetScrollbar(scrollbar);
        }
        // Populate the companion list with checkboxes
        public void PopulateCompanionList(List<Companion> companions, Player player)
        {
            companionList.Clear();
            foreach (var companion in companions)
            {
                var checkbox = new UICheckbox(companion.ID, companion.Active);
                checkbox.OnCheckedChanged += (selected) =>
                {
                    companion.Active = selected;
                    if (selected)
                    {
                        companion.Spawn(player);
                    }
                    else
                    {
                        companion.Despawn();
                    }
                };
                companionList.Add(checkbox);
            }
        }
    }
}
