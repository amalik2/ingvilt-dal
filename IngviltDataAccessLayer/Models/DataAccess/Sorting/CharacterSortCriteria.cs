using System.Collections.Generic;

namespace Ingvilt.Models.DataAccess.Sorting {
    public class CharacterSortCriteria : BaseSortCriteria {
        public CharacterSortCriteria(bool ascending) : base(ascending) {
        }

        protected override List<string> GetSortQueryColumnNames() {
            return new List<string>() { "c.character_id" };
        }
    }
}
