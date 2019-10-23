using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Items {
    public class ItemInfoContainer {

        public static Dictionary<ItemEnum, DescriptionContainer> infoDictionary = new Dictionary<ItemEnum, DescriptionContainer> {
            { ItemEnum.NONE, new DescriptionContainer("None", "This only exists for development purposes.  If you see it anywhere, please submit a bug report.", ItemEnum.NONE, 0) },
            { ItemEnum.BASIC_SHOES, new DescriptionContainer("Basic Shoes", "Increases your movement speed by 10%.", ItemEnum.NONE, 10) },
            { ItemEnum.SNEAKERS, new DescriptionContainer("Sneakers", "Increases your movement speed by 15% and reduces noise range of jumping and dashing.", ItemEnum.BASIC_SHOES, 20) },
            { ItemEnum.RUNNING_SHOES, new DescriptionContainer("Sneakers", "Increases your movement speed by 25%.", ItemEnum.BASIC_SHOES, 20) },
            { ItemEnum.BASKET_BALL_SHOES, new DescriptionContainer("Sneakers", "Increases your movement speed by 15% and increases jump height by 25%.", ItemEnum.BASIC_SHOES, 20) },
            { ItemEnum.WIND_STONE, new DescriptionContainer("Wind Stone", "Increases your movement speed by 10%", ItemEnum.NONE, 10) },
            { ItemEnum.TORNADO_STONE, new DescriptionContainer("Tornado Stone", "Increases your movement speed by 15% and acceleration by 10%", ItemEnum.WIND_STONE, 20) },
            { ItemEnum.GALE_STONE, new DescriptionContainer("Gale Stone", "Increases your movement speed by 25%", ItemEnum.WIND_STONE, 20) },
        };

        public class DescriptionContainer {

            public String Name { get; set; }
            public String Description { get; set; }
            public ItemEnum RequiredItem { get; set; }
            public int Cost { get; set; }

            public DescriptionContainer(string name, string description, ItemEnum requiredItem, int cost) {
                Name = name;
                Description = description;
                RequiredItem = requiredItem;
                Cost = cost;
            }
        }
    }
}
