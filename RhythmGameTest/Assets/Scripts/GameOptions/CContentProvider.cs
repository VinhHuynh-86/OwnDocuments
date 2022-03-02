using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class CInnerContainer<T>
	{
        Dictionary<string, T>	m_Container = new Dictionary<string, T>();

		public bool SetValue(string _name, T value)
		{
            bool found = false;
			if(m_Container.Count > 0)
            {
                foreach(string m_name in m_Container.Keys)
                {
                    if(m_name == _name)
                    {
                        found = true;
                        m_Container[m_name] = value;
                        return found; // found
                    }
                }
            }
            
            if(!found)
            {
                m_Container.Add(_name, value);
            }
			return false;
		}

		public T GetValue(string _name, T defaultValue)
		{
            if(m_Container.Count > 0)
            {
                foreach(string m_name in m_Container.Keys)
                {
                    if(m_name == _name)
                    {
                        return m_Container[_name]; // found
                    }
                }
            }
            
            {
                return defaultValue;
            }
		}

		public Dictionary<string, T> GetFullMap()
		{
			return m_Container;
		}

	};


	public class CContentProvider
	{
        public CInnerContainer<int>	m_IntContainer = new CInnerContainer<int>();
		public CInnerContainer<float> m_FloatContainer = new CInnerContainer<float>();
		public CInnerContainer<bool> m_BoolContainer = new CInnerContainer<bool>();
		public CInnerContainer<string> m_StringContainer = new CInnerContainer<string>();

        public CContentProvider()
        {

        }
		public bool SetValue(string _name, int value)
        {
            return m_IntContainer.SetValue(_name, value);
        }
		public int GetValue(string _name, int defaultValue)
        {
            return m_IntContainer.GetValue(_name, defaultValue);
        }

		public bool SetValue(string _name, float value)
        {
            return m_FloatContainer.SetValue(_name, value);
        }
		public float GetValue(string _name, float defaultValue)
        {
            return m_FloatContainer.GetValue(_name, defaultValue);
        }

		public bool SetValue(string _name, bool value)
        {
            return m_BoolContainer.SetValue(_name, value);
        }
		public bool GetValue(string _name, bool defaultValue)
        {
            return m_BoolContainer.GetValue(_name, defaultValue);
        }
		public bool SetValue(string _name, string value)
        {
            return m_StringContainer.SetValue(_name, value);
        }
		public string GetValue(string _name, string defaultValue)
        {
            return m_StringContainer.GetValue(_name, defaultValue);
        }  
			
		public Dictionary<string, int> GetIntContainerByReference()
		{ 
			return m_IntContainer.GetFullMap(); 
		}

		public Dictionary<string, float> GetFloatContainerByReference() 
		{ 
			return m_FloatContainer.GetFullMap(); 
		}

		public Dictionary<string, bool> GetBoolContainerByReference() 
		{ 
			return m_BoolContainer.GetFullMap(); 
		}

		public Dictionary<string, string> GetStringContainerByReference()
		{ 
			return m_StringContainer.GetFullMap(); 
		}
	};


