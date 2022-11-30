using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JaimesUtilities { 
    public class AutoResetBool 
    {
        public bool value {
            get {
                return _time > Time.time;
            }
            set {
                if (value) _time = Time.time + _resetTime;
                else _time = -1;
            }
        }

        private float _time = -1;
        private float _resetTime;

        public void SetTrue(float resetTime = -1f) {
            if (resetTime != -1) Edit(resetTime);
            value = true;
        }
        public void SetFalse(float resetTime = -1f) {
            if (resetTime != -1) Edit(resetTime);
            value = false;
        }
        public void Edit(float resetTime = 1f) {
            _resetTime = resetTime;
        } 

        public AutoResetBool(bool value, float resetTime = 1f) {
            Edit(resetTime);
            this.value = value;
        }
        public AutoResetBool(float resetTime = 1f) {
            Edit(resetTime);
        }
    }
}
