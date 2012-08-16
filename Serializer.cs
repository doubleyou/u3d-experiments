using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

[AttributeUsage(AttributeTargets.All)]
public class Binarizable : Attribute {
	public int serializationOrder;
	public Binarizable(int num) {
		serializationOrder = num;
	}
	public Binarizable() {
		serializationOrder = 0;
	}
}

public class Serializer {
	public static byte[] Serialize(object obj) {
		byte[] ret = null;
		
		FieldInfo[] fields = obj.GetType().GetFields();
	
		object[] values = new object[fields.Length];
		
		int totalFields = 0;
		foreach(FieldInfo field in fields) {
			object[] attrs = field.GetCustomAttributes(false);
			foreach(Attribute attr in attrs) {
					if(attr is Binarizable) {
					Binarizable a = attr as Binarizable;
					values[a.serializationOrder - 1] = field.GetValue(obj);
					totalFields++;
				}
			}
		}
		
		byte[][] buffers = new byte[totalFields][];
		int totalByteSize = 0;
		
		for(int i = 0; i < totalFields; i++) {
			var v = values[i];
			if (v is float) {
				buffers[i] = System.BitConverter.GetBytes((float)v);
			} else if (v is bool) {
				buffers[i] = System.BitConverter.GetBytes((bool)v);
			} else if (v is int) {
				buffers[i] = System.BitConverter.GetBytes((int)v);
			} else if (v is string) {
				string str = v as string;
				buffers[i] = new byte[str.Length+1];
				byte[] tmp = System.Text.Encoding.ASCII.GetBytes(str);
				System.Buffer.BlockCopy(tmp, 0, buffers[i], 1, str.Length);
				buffers[i][0] = (byte)str.Length;
			} else if (v is Vector3) {
				Vector3 vect = (Vector3)v;
				byte[] bX = System.BitConverter.GetBytes(vect.x);
				byte[] bY = System.BitConverter.GetBytes(vect.y);
				byte[] bZ = System.BitConverter.GetBytes(vect.z);
				buffers[i] = new byte[12];
				System.Buffer.BlockCopy(bX, 0, buffers[i], 0, 4);
				System.Buffer.BlockCopy(bY, 0, buffers[i], 4, 4);
				System.Buffer.BlockCopy(bZ, 0, buffers[i], 8, 4);
			} else {
				Debug.Log (v.GetType());
				throw(new Exception("Unknown serialization type"));
			}
			totalByteSize += buffers[i].Length;
		}
		
		ret = new byte[totalByteSize + 1];
		int offset = 1;
		
		for (int i = 0; i < totalFields; i++) {
			System.Buffer.BlockCopy(buffers[i], 0, ret, offset, buffers[i].Length);
			offset += buffers[i].Length;
		}
		
		foreach(Attribute attr in obj.GetType().GetCustomAttributes(false)) {
			if (attr is Binarizable) {
				Binarizable a = attr as Binarizable;
				ret[0] = (byte)a.serializationOrder;
			}
		}
		
		return ret;
	}
	
	public static int Unserialize(byte[] src) {
	
		int offset = 1;
		
		byte operation = src[0];
		
		if (operation == 0) return 0;
		
		while(offset < src.Length) {
			int objectID = System.BitConverter.ToInt32(src, offset+=4);
			offset = LoadObject(objectID, src, offset);
		}
		
		return 1;
		
	}
	
	static int LoadObject(int id, byte[] src, int offset) {
		
		//FIXME: need to handle custom classes here
		object target = GameObject.Find(id.ToString()).GetComponent<Character>();
		
		FieldInfo[] fields = new FieldInfo[target.GetType().GetFields().Length];
		
		int totalFields = 0;
		
		foreach(FieldInfo field in target.GetType().GetFields()) {
			object[] attrs = field.GetCustomAttributes(false);
			foreach(Attribute attr in attrs) {
				if(attr is Binarizable) {
					Binarizable a = attr as Binarizable;
					fields[a.serializationOrder-1] = field;
					totalFields++;
				}
			}
		}
		
		foreach(FieldInfo field in fields) {
			if (field == null) continue;
			if (field.GetType() == typeof(int)) {
				int v = System.BitConverter.ToInt32(src, offset+=4);
				field.SetValue(target, v);
			} else if (field.GetType() == typeof(float)) {
				float v = System.BitConverter.ToSingle(src, offset+=4);
				field.SetValue(target, v);
			} else if (field.GetType() == typeof(bool)) {
				bool v = System.BitConverter.ToBoolean(src, offset++);
				field.SetValue(target, v);
			} else if (field.GetType() == typeof(string)) {
				byte length = src[offset++];
				byte[] buf = new byte[length];
				System.Buffer.BlockCopy(src, offset+=length, buf, 0, length);
				string v = System.Text.Encoding.ASCII.GetString(buf);
				field.SetValue(target, v);
			} else if (field.GetType() == typeof(Vector3)) {
				float x = System.BitConverter.ToSingle(src, offset+=4);
				float y = System.BitConverter.ToSingle(src, offset+=4);
				float z = System.BitConverter.ToSingle(src, offset+=4);
				Vector3 v = new Vector3(x, y, z);
				field.SetValue(target, v);
			}
		}
		
		return offset;
	}
	
}
