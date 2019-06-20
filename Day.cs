using System;

namespace sqlite {
    class Day {
        private double hour;
        private DateTime date;

        public Day(double hour, DateTime date) {
            this.hour = hour;
            this.date = date;
        }

        public double Hour {
            get {
                return this.hour;
            }
        }

        public DateTime Date {
            get {
                return this.date;
            }
        }
    }
}