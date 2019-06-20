using System;
using System.Collections.Generic;

namespace sqlite {
    class Log {
        private List<Day> days_list;

        public List<Day> days {
            get {
                return this.days_list;
            }
        }

        public void Add(Day day) {
            this.days_list.Add(day);
        }
    }
}