namespace Com.Tempest.Nightmare {

    public class TalentBase {
        
        protected string Name { get; set; }
        protected int RarityLevel { get; set; }
        protected int MaxLevel { get; set; }

        public TalentBase(string name) {
            Name = name;
        }
    }
}
