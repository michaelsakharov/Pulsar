using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.IO;

using Duality;
using Duality.Drawing;
using Duality.Resources;
using Duality.Components;
using NUnit.Framework;


namespace Duality.Tests.Resources
{
	[TestFixture]
	public class PrefabTest
	{
		private HashSet<Resource> localTempContent = new HashSet<Resource>();

		[TearDown] public void TearDownTest()
		{
			foreach (Resource res in localTempContent)
			{
				ContentProvider.RemoveContent(res);
			}
			localTempContent.Clear();
		}

		[Test] public void CreatePrefab()
		{
			GameObject obj = this.CreateSimpleGameObject();

			Prefab prefab = new Prefab(obj);
			Assert.IsTrue(prefab.ContainsData);

			GameObject instance = prefab.Instantiate();
			Assert.AreNotSame(obj, instance);
			Assert.IsTrue(this.CheckEquality(instance, obj));
		}
		[Test] public void ApplyPrefab()
		{
			GameObject prefabContent = this.CreateSimpleGameObject();
			Prefab prefab = new Prefab(prefabContent);
			GameObject obj = new GameObject();

			obj.LinkToPrefab(prefab);
			Assert.AreEqual(prefab, obj.PrefabLink.Prefab.Res);
			Assert.AreEqual(obj, obj.PrefabLink.Obj);
			Assert.IsFalse(this.CheckEquality(obj, prefabContent));

			obj.PrefabLink.Apply();
			Assert.AreEqual(prefab, obj.PrefabLink.Prefab.Res);
			Assert.AreEqual(obj, obj.PrefabLink.Obj);
			Assert.AreNotSame(prefabContent, obj);
			Assert.IsTrue(this.CheckEquality(obj, prefabContent));
		}
		[Test] public void ApplyPrefabKeepsObjects()
		{
			GameObject prefabContent = this.CreateSimpleGameObject();
			this.CreateSimpleGameObject(prefabContent);

			Prefab prefab = new Prefab(prefabContent);
			GameObject obj = prefab.Instantiate();
			obj.LinkToPrefab(prefab);

			Camera sprite = obj.GetComponent<Camera>();
			Assert.AreNotSame(sprite, prefabContent.GetComponent<Camera>());

			GameObject child = obj.Children.First();
			Assert.AreNotSame(child, prefabContent.Children.First());

			obj.PrefabLink.Apply();
			Assert.AreSame(sprite, obj.GetComponent<Camera>());
			Assert.AreSame(child, obj.Children.First());
			Assert.AreNotSame(sprite, prefabContent.GetComponent<Camera>());
			Assert.AreNotSame(child, prefabContent.Children.First());
		}
		[Test] public void ApplyChanges()
		{
			GameObject prefabContent = this.CreateSimpleGameObject();
			this.CreateSimpleGameObject(prefabContent);
			Prefab prefab = new Prefab(prefabContent);

			GameObject obj = prefab.Instantiate();
			Camera sprite = obj.GetComponent<Camera>();
			Camera childSprite = obj.Children.First().GetComponent<Camera>();

			obj.LinkToPrefab(prefab);

			sprite.useCustomViewPort = true;
			childSprite.useCustomViewPort = true;

			obj.PrefabLink.PushChange(sprite, PropertyOf(() => sprite.useCustomViewPort));
			obj.PrefabLink.PushChange(childSprite, PropertyOf(() => sprite.useCustomViewPort));

			obj.PrefabLink.ApplyPrefab();
			Assert.AreNotEqual(sprite.useCustomViewPort, true);
			Assert.AreNotEqual(childSprite.useCustomViewPort, true);

			obj.PrefabLink.ApplyChanges();
			Assert.AreEqual(sprite.useCustomViewPort, true);
			Assert.AreEqual(childSprite.useCustomViewPort, true);
		}
		[Test] public void AllowAdditionalObjects()
		{
			GameObject prefabContent = this.CreateSimpleGameObject();
			Prefab prefab = new Prefab(prefabContent);

			GameObject obj = prefab.Instantiate();
			obj.AddComponent<TestComponent>();
			this.CreateSimpleGameObject(obj);

			obj.LinkToPrefab(prefab);
			obj.PrefabLink.Apply();
			Assert.IsNotNull(obj.GetComponent<TestComponent>());
			Assert.IsTrue(obj.Children.Any());
		}
		[TestCase(true), TestCase(false)]
		[Test] public void TransformHierarchyPrefabSceneBug(bool childPrefab)
		{
			// Tests for https://github.com/AdamsLair/duality/issues/53

			string prefabName = "TestPrefab";
			string parentName = "Parent";
			string childName = "Child";
			Vector3 parentPos = new Vector3(100, 0, 0);
			Vector3 childPos = new Vector3(200, 0, 0);

			// Create object hierarchy as described
			Scene scene = new Scene();
			GameObject parent = new GameObject(parentName);
			parent.AddComponent<Transform>();
			parent.Transform.Pos = parentPos;
			GameObject child = new GameObject(childName, parent);
			child.AddComponent<Transform>();
			child.Transform.Pos = childPos;
			scene.AddObject(parent);

			// Create a Prefab from this hierarchy, make it available and link to it
			Prefab prefab = new Prefab(childPrefab ? child : parent);
			this.AddTempContent(prefabName, prefab);
			(childPrefab ? child : parent).LinkToPrefab(prefab);

			// Save the Scene and reload it
			using (MemoryStream stream = new MemoryStream())
			{
				scene.Save(stream);
				stream.Position = 0;
				scene = Resource.Load<Scene>(stream);
			}

			// Gather new object references
			parent = scene.FindGameObject(parentName);
			child = scene.FindGameObject(childName);

			// Check if positions are correct
			Assert.AreEqual(parentPos, parent.Transform.Pos);
			Assert.AreEqual(childPos, child.Transform.Pos);
		}
		[Test] public void NestedPrefabChangeListPreservation()
		{
			GameObject parentObj = this.CreateSimpleGameObject();
			GameObject childObj = this.CreateSimpleGameObject(parentObj);
			Prefab childPrefab = new Prefab(childObj);
			childObj.LinkToPrefab(childPrefab);
			Prefab parentPrefab = new Prefab(parentObj);
			parentObj.LinkToPrefab(parentPrefab);

			Camera childSprite = childObj.GetComponent<Camera>();
			childSprite.useCustomViewPort = true;

			childObj.PrefabLink.PushChange(childSprite, PropertyOf(() => childSprite.useCustomViewPort));
			parentObj.PrefabLink.PushChange(childObj, PropertyOf(() => childObj.PrefabLink));

			// Apply the parent Prefab. In an error case, this could overwrite the childs PrefabLink
			parentObj.PrefabLink.Apply();
			// Apply it again. If the childs PrefabLink was overwritten, this will change the child sprites color
			parentObj.PrefabLink.Apply();

			Assert.AreEqual(true, childSprite.useCustomViewPort);
		}

		[Test] public void SameNamePropertyChanges()
		{
			var gameObj = this.CreateSimpleGameObject();
			var prefab = new Prefab(gameObj);
			gameObj.LinkToPrefab(prefab);

			var transform = gameObj.GetComponent<Transform>();
			var spriteRenderer = gameObj.GetComponent<Camera>();
			gameObj.PrefabLink.PushChange(transform, PropertyOf(() => transform.ActiveSingle), false);
			gameObj.PrefabLink.PushChange(spriteRenderer, PropertyOf(() => spriteRenderer.ActiveSingle), false);

			gameObj.PrefabLink.ApplyChanges();

			Assert.IsFalse(gameObj.GetComponent<Camera>().ActiveSingle);
			Assert.IsFalse(gameObj.GetComponent<Transform>().ActiveSingle);
		}
		[Test] public void PrefabChangeListCloningBug()
		{
			// Tests for https://github.com/AdamsLair/duality/issues/191
			
			// Create a sample Scene to test this
			GameObject objA = new GameObject("ObjectA");
			GameObject objB = new GameObject("ObjectB");
			TestReferenceComponent refComp = objA.AddComponent<TestReferenceComponent>();
			List<GameObject> objList = refComp.ReferencedObjectList;

			// Create a Prefab, make it available and link to it
			Prefab prefab = new Prefab(objA);
			this.AddTempContent("TestPrefab", prefab);
			objA.LinkToPrefab(prefab);

			// Assign a new reference to the Prefab instance
			refComp.ReferencedObject = objB;
			refComp.ReferencedObjectList.Add(objB);
			objA.PrefabLink.PushChange(refComp, PropertyOf(() => refComp.ReferencedObject));
			objA.PrefabLink.PushChange(refComp, PropertyOf(() => refComp.ReferencedObjectList));

			// Are we pointing to the right object?
			Assert.AreSame(objB, refComp.ReferencedObject);
			Assert.AreSame(objB, refComp.ReferencedObjectList[0]);
			Assert.AreSame(objList, refComp.ReferencedObjectList);

			// Now apply the Prefab
			objA.PrefabLink.Apply();

			// Are we still pointing to the right object? Or is it a copy now?
			Assert.AreEqual(objB.Name, refComp.ReferencedObject.Name);
			Assert.AreEqual(objB.Name, refComp.ReferencedObjectList[0].Name);
			Assert.AreSame(objB, refComp.ReferencedObject);
			Assert.AreSame(objB, refComp.ReferencedObjectList[0]);
			// Have we still properly copied the list / container?
			Assert.AreNotSame(objList, refComp.ReferencedObjectList);
		}

		private void AddTempContent(string path, Resource res)
		{
			ContentProvider.AddContent(path, res);
			localTempContent.Add(res);
		}
		private GameObject CreateSimpleGameObject(GameObject parent = null)
		{
			GameObject obj = new GameObject("SimpleObject", parent);
			obj.AddComponent<Transform>();
			obj.AddComponent<Camera>();
			return obj;
		}
		private bool CheckEquality(GameObject a, GameObject b)
		{
			if (a.Name != b.Name) return false;
			if (a.ActiveSingle != b.ActiveSingle) return false;
			if (a.Children.Count != b.Children.Count) return false;
			if (a.Components.Count != b.Components.Count) return false;

			foreach (Component ca in a.Components)
			{
				Component cb = b.GetComponent(ca.GetType());
				if (cb == null) return false;
				if (!this.CheckEquality(ca, cb)) return false;
			}

			for (int i = 0; i < a.Children.Count; i++)
			{
				GameObject ca = a.Children[i];
				GameObject cb = b.Children[i];
				if (!this.CheckEquality(ca, cb)) return false;
			}

			return true;
		}
		private bool CheckEquality(Component a, Component b)
		{
			Type type = a.GetType();

			foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				TypeInfo propertyTypeInfo = property.PropertyType.GetTypeInfo();
				if (!propertyTypeInfo.IsDeepCopyByAssignment()) continue;

				object va = property.GetValue(a, null);
				object vb = property.GetValue(b, null);

				if (!object.Equals(va, vb)) return false;
			}

			return true;
		}

		private static PropertyInfo PropertyOf<T>(Expression<Func<T>> expression) {
			var body = (MemberExpression)expression.Body;
			return (PropertyInfo)body.Member;
		}

		private class TestComponent : Component {}
		private class TestReferenceComponent : Component
		{
			private GameObject obj;
			private List<GameObject> objList = new List<GameObject>();

			public GameObject ReferencedObject
			{
				get { return this.obj; }
				set { this.obj = value; }
			}
			public List<GameObject> ReferencedObjectList
			{
				get { return this.objList; }
				set { this.objList = value; }
			}
		}
	}
}
