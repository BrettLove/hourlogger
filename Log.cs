using System;
using System.Collections.Generic;

namespace hourlogger {
    class Log {
        private List<Day> days_list;

        public Log() {
            this.days_list = new List<Day>();
        }

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