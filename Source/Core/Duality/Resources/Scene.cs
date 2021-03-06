using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Duality.Editor;
using Duality.Components;
using Duality.Cloning;
using Duality.Properties;
using Duality.Utility.Coroutines;
using Duality.Drawing;
using System.Collections;
using THREE.Core;

namespace Duality.Resources
{
	/// <summary>
	/// A Scene encapsulates an organized set of <see cref="GameObject">GameObjects</see> and provides
	/// update-, rendering- and maintenance functionality. In Duality, there is always exactly one Scene
	/// <see cref="Scene.Current"/> which represents a level, gamestate or a combination of both, depending
	/// on you own design.
	/// </summary>
	[EditorHintCategory(CoreResNames.CategoryNone)]
	[EditorHintImage(CoreResNames.ImageScene)]
	public sealed class Scene : Resource
	{
		private static ContentRef<Scene>			current           = null;
		private static bool							curAutoGen        = false;
		private static bool							isSwitching       = false;
		private static int							switchLock        = 0;
		private static bool							switchToScheduled = false;
		private static ContentRef<Scene>			switchToTarget    = null;
		private static THREE.Scenes.Scene			threeScene		  = null;
		private static Camera						camera			  = null;
		private static Dictionary<int, GameObject>	threeIDs		  = null;

		/// <summary>
		/// [GET / SET] The Scene that is currently active i.e. updated and rendered. This is never null.
		/// You may assign null in order to leave the current Scene and enter en empty dummy Scene.
		/// </summary>
		public static Scene Current
		{
			get
			{
				if (!curAutoGen && !current.IsAvailable)
				{
					curAutoGen = true;
					Current = new Scene();
					curAutoGen = false;
				}
				return current.Res;
			}
			private set
			{
				// Only perform a scene switch if current and next are different
				if (current.ResWeak != value)
				{
					switchLock++;
					try
					{
						if (Leaving != null)
							Leaving(current, null);

						isSwitching = true;

						if (current.ResWeak != null && current.ResWeak.IsActive)
							current.ResWeak.Deactivate();

						current.Res = value ?? new Scene();

						if (current.ResWeak != null && !current.ResWeak.IsActive)
							current.ResWeak.Activate();

						isSwitching = false;

						if (Entered != null)
							Entered(current, null);
					}
					finally
					{
						switchLock--;
						isSwitching = false;
					}
				}
				// If the scene is actually the same, we still do the assignment to
				// make sure our internal ContentRef will update its resource path,
				// should this have changed. This can happen during a rename event in
				// the editor.
				else
				{
					current.Res = value ?? new Scene();
				}
			}
		}
		/// <summary>
		/// [GET] The Resource file path of the current Scene.
		/// </summary>
		public static string CurrentPath
		{
			get { return current.Res != null ? current.Res.Path : current.Path; }
		}
		/// <summary>
		/// [GET] Returns whether <see cref="Scene.Current"/> is in a transition between two different states, i.e.
		/// whether the current Scene is being changed right now.
		/// </summary>
		public static bool IsSwitching
		{
			get { return isSwitching; }
		}
		public static THREE.Scenes.Scene ThreeScene
		{
			get { return threeScene; }
		}
		public static Dictionary<int, GameObject> ThreeIDs
		{
			get { return threeIDs; }
		}
		public static Camera Camera
		{
			get { return camera; }
			set { camera = value; }
		}


		/// <summary>
		/// Fired just before leaving the current Scene.
		/// </summary>
		public static event EventHandler Leaving;
		/// <summary>
		/// Fired right after entering the (now) current Scene.
		/// </summary>
		public static event EventHandler Entered;
		/// <summary>
		/// Fired when a <see cref="GameObject">GameObjects</see> parent object has been changed in the current Scene.
		/// </summary>
		public static event EventHandler<GameObjectParentChangedEventArgs> GameObjectParentChanged;
		/// <summary>
		/// Fired once every time a group of <see cref="GameObject"/> instances has been registered in the current Scene.
		/// </summary>
		public static event EventHandler<GameObjectGroupEventArgs> GameObjectsAdded;
		/// <summary>
		/// Fired once every time a group of <see cref="GameObject"/> instances has been unregistered from the current Scene.
		/// </summary>
		public static event EventHandler<GameObjectGroupEventArgs> GameObjectsRemoved;
		/// <summary>
		/// Fired when a <see cref="Component"/> has been added to a <see cref="GameObject"/> that is registered in the current Scene.
		/// </summary>
		public static event EventHandler<ComponentEventArgs> ComponentAdded;
		/// <summary>
		/// Fired when a <see cref="Component"/> has been removed from a <see cref="GameObject"/> that is registered in the current Scene.
		/// </summary>
		public static event EventHandler<ComponentEventArgs> ComponentRemoving;


		static Scene()
		{
			threeScene = new  THREE.Scenes.Scene();
			threeIDs = new Dictionary<int, GameObject>();
			Current = new Scene();
		}

		/// <summary>
		/// Switches to the specified <see cref="Scene"/>, which will become the new <see cref="Current">current one</see>.
		/// By default, this method does not guarantee to perform the Scene switch immediately, but may defer the switch
		/// to the end of the current update cycle.
		/// </summary>
		/// <param name="scene">The Scene to switch to.</param>
		/// <param name="forceImmediately">If true, an immediate switch is forced. Use only when necessary.</param>
		public static void SwitchTo(ContentRef<Scene> scene, bool forceImmediately = false)
		{
			// Check parameters
			if (!scene.IsExplicitNull && !scene.IsAvailable) 
				throw new ArgumentException("Can't switch to Scene '" + scene.Path + "' because it doesn't seem to exist.", "scene");

			// Check whether there is anything that would prevent us from doing the
			// switch right now, instead of at the end of the current frame
			bool switchIsBlocked =
				switchLock != 0 ||
				Scene.Current.IsRenderingOrUpdating ||
				(scene.Res != null && scene.Res.IsRenderingOrUpdating);

			if (switchIsBlocked && !forceImmediately)
			{
				switchToTarget = scene;
				switchToScheduled = true;
			}
			else
			{
				if (threeScene != null) threeScene.Dispose();
				threeScene = new THREE.Scenes.Scene();
				threeIDs = new Dictionary<int, GameObject>();
				Scene.Current = scene.Res;
			}
		}
		/// <summary>
		/// Reloads the <see cref="Current">current Scene</see> or schedules it for reload at the end of the
		/// frame, depending on whether it is considered safe to do so immediately. Similar to <see cref="SwitchTo"/> with
		/// regard to execution planning.
		/// </summary>
		public static void Reload()
		{
			ContentRef<Scene> target = Scene.Current;

			// Check whether there is anything that would prevent us from doing the
			// reload right now, instead of at the end of the current frame
			bool reloadIsBlocked =
				switchLock != 0 ||
				Scene.Current.IsRenderingOrUpdating;

			if (reloadIsBlocked)
				Scene.Current.DisposeLater();
			else
				Scene.Current.Dispose();

			Scene.SwitchTo(target);
		}

		/// <summary>
		/// Performs a <see cref="Scene"/> switch operation that was scheduled using
		/// <see cref="Scene.SwitchTo"/>.
		/// </summary>
		internal static bool PerformScheduledSwitch()
		{
			if (!switchToScheduled) return false;

			// Retrieve the target and reset the scheduled switch
			string oldName = Scene.Current.FullName;
			Scene target = switchToTarget.Res;
			switchToTarget = null;
			switchToScheduled = false;

			// Perform the scheduled switch
			if (threeScene != null) threeScene.Dispose();
			threeScene = new THREE.Scenes.Scene();
			threeIDs = new Dictionary<int, GameObject>();
			Scene.Current = target;

			// If we now end up with another scheduled switch, we might be
			// caught up in a redirect loop, where a Scene, when activated,
			// will immediately switch to another Scene, which will do the same.
			if (switchToScheduled)
			{
				switchToTarget = null;
				switchToScheduled = false;
				Logs.Core.WriteWarning(
					"Potential Scene redirect loop detected: When performing previously " +
					"scheduled switch to Scene '{0}', a awitch to Scene '{1}' was immediately scheduled. " +
					"The second switch will not be performed to avoid entering a loop. Please " +
					"check when you call Scene.SwitchTo and avoid doing that during object activation.",
					oldName, Scene.Current.FullName);
			}

			return true;
		}

		public static void AddToThreeScene(Object3D obj3D, GameObject Owner)
		{
			threeScene.Add(obj3D);
			if (!threeIDs.ContainsKey(obj3D.Id))
				threeIDs.Add(obj3D.Id, Owner);
		}

		public static void RemoveFromThreeScene(Object3D obj3D)
		{
			threeScene.Remove(obj3D);
			if (threeIDs.ContainsKey(obj3D.Id))
				threeIDs.Remove(obj3D.Id);
		}

		public static GameObject GetGameobjectByThreeID(int ThreeID)
		{
			return threeIDs.ContainsKey(ThreeID) ? threeIDs[ThreeID] : null;
		}

		public static List<int> GetThreeIDsByGameObject(GameObject gameObj)
		{
			// TODO: This should probably keep track of Gameobjects and IDs would save on some performance
			List<int> objectsFound = new List<int>();
			foreach(int id in threeIDs.Keys)
				if (threeIDs[id] == gameObj)
					objectsFound.Add(id);
			return objectsFound;
		}

		private struct UpdateEntry
		{
			public TypeInfo Type;
			public int Count;
			public TimeCounter Profiler;
		}

		private Vector2                     globalGravity      = Vector2.UnitY * 33.0f;
		private GameObject[]                serializeObj       = null;

		private ColorRgba                   backgroundColor	   = ColorRgba.Black;
		private ColorRgba                   fogColor		   = ColorRgba.Grey;
		private float						fogNear			   = 10f;
		private float						fogFar			   = 30f;
		private bool						moveWorldInstead   = true;

		[DontSerialize] private bool active = false;
		[DontSerialize] private bool isUpdating = false;
		[DontSerialize] private bool isRendering = false;

		[DontSerialize]
		[CloneField(CloneFieldFlags.DontSkip)]
		[CloneBehavior(typeof(GameObject), CloneBehavior.ChildObject)]
		private	GameObjectManager objectManager = new GameObjectManager();

		[DontSerialize]
		[CloneField(CloneFieldFlags.DontSkip)]
		private Dictionary<TypeInfo,List<Component>> componentsByType = new Dictionary<TypeInfo,List<Component>>();

		// Temporary buffers used during scene updates, stored and re-used for efficiency
		[DontSerialize] private List<Type> updateTypeOrder = new List<Type>();
		[DontSerialize] private RawList<Component> updatableComponents = new RawList<Component>(256);
		[DontSerialize] private RawList<UpdateEntry> updateMap = new RawList<UpdateEntry>();

		[DontSerialize] private readonly CoroutineManager coroutineManager = new CoroutineManager();

		/// <summary>
		/// [GET] Returns the <see cref="Coroutine"/> manager for this <see cref="Scene"/>.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public CoroutineManager CoroutineManager
		{
			get { return this.coroutineManager; }
		}
		/// <summary>
		/// [GET] Enumerates all registered objects.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public IEnumerable<GameObject> AllObjects
		{
			get { return this.objectManager.AllObjects; }
		}
		/// <summary>
		/// [GET] Enumerates all registered objects that are currently active.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public IEnumerable<GameObject> ActiveObjects
		{
			get { return this.objectManager.ActiveObjects; }
		}
		/// <summary>
		/// [GET] Enumerates all root GameObjects, i.e. all GameObjects without a parent object.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public IEnumerable<GameObject> RootObjects
		{
			get { return this.objectManager.RootObjects; }
		}
		/// <summary>
		/// [GET] Enumerates all <see cref="RootObjects"/> that are currently active.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public IEnumerable<GameObject> ActiveRootObjects
		{
			get { return this.objectManager.ActiveRootObjects; }
		}
		/// <summary>
		/// [GET] Returns whether this Scene is <see cref="Scene.Current"/>.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public bool IsCurrent
		{
			get { return current.ResWeak == this; }
		}
		/// <summary>
		/// [GET] Returns whether this <see cref="Scene"/> is currently <see cref="Activate">active</see>,
		/// i.e. in a state where it can update game simulation and be rendered.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public bool IsActive
		{
			get { return this.active; }
		}
		/// <summary>
		/// [GET] Returns whether this Scene is completely empty.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public bool IsEmpty
		{
			get { return !this.objectManager.AllObjects.Any(); }
		}
		private bool IsRenderingOrUpdating
		{
			get { return this.isRendering || this.isUpdating; }
		}
		public ColorRgba BackgroundColor
		{
			get { return this.backgroundColor; }
			set { this.backgroundColor = value; }
		}
		public ColorRgba FogColor
		{
			get { return this.fogColor; }
			set { this.fogColor = value; }
		}
		public float FogNear
		{
			get { return this.fogNear; }
			set { this.fogNear = value; }
		}
		public float FogFar
		{
			get { return this.fogFar; }
			set { this.fogFar = value; }
		}

		public bool MoveWorldInsteadOfCamera
		{
			get { return this.moveWorldInstead; }
			set { this.moveWorldInstead = value; }
		}


		/// <summary>
		/// Creates a new, empty scene which does not contain any <see cref="GameObject">GameObjects</see>.
		/// </summary>
		public Scene()
		{
			this.RegisterManagerEvents();
		}

		/// <summary>
		/// Transitions the <see cref="Scene"/> into an active state, where it can be 
		/// updated and rendered. This is the state of a <see cref="Scene"/> while it 
		/// is <see cref="Current"/>.
		/// </summary>
		public void Activate()
		{
			if (this.active) throw new InvalidOperationException("Cannot activate a scene that is already active.");

			// Set state to active immediately, so the scene will be treated as
			// such when reacting to added objects and similar events.
			this.active = true;

			//ThreeScene.Fog = new THREE.Scenes.Fog(System.Drawing.Color.FromArgb(255, FogColor.R, FogColor.G, FogColor.B), FogNear, FogFar);
			ThreeScene.Background = new THREE.Math.Color(BackgroundColor.R / 255f, BackgroundColor.G / 255f, BackgroundColor.B / 255f);

			// When in the editor, apply prefab links
			if (DualityApp.ExecEnvironment == DualityApp.ExecutionEnvironment.Editor)
				this.ApplyPrefabLinks();

			// When running the game, break prefab links
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Game)
				this.BreakPrefabLinks();

			// Activate GameObjects
			DualityApp.EditorGuard(() =>
			{
				// Create a list of components to activate
				List<ICmpInitializable> initList = new List<ICmpInitializable>();
				foreach (Component component in this.FindComponents<ICmpInitializable>())
				{
					if (!component.Active) continue;
					initList.Add(component as ICmpInitializable);
				}
				// Activate all the listed components. Note that they may create or destroy
				// objects, so it's important that we're iterating a copy of the scene objects
				// here, and not the real thing.
				for (int i = 0; i < initList.Count; i++)
				{
					initList[i].OnActivate();
				}
			});
		}
		/// <summary>
		/// Transitions the <see cref="Scene"/> into an inactive state, where it can no 
		/// longer be updated or rendered. This is the state of a <see cref="Scene"/> 
		/// after it was loaded.
		/// </summary>
		public void Deactivate()
		{
			if (!this.active) throw new InvalidOperationException("Cannot deactivate a scene that is not active.");

			// Deactivate GameObjects
			DualityApp.EditorGuard(() =>
			{
				// Create a list of components to deactivate
				List<ICmpInitializable> shutdownList = new List<ICmpInitializable>();
				foreach (Component component in this.FindComponents<ICmpInitializable>())
				{
					if (!component.Active) continue;
					shutdownList.Add(component as ICmpInitializable);
				}
				// Deactivate all the listed components. Note that they may create or destroy
				// objects, so it's important that we're iterating a copy of the scene objects
				// here, and not the real thing.
				for (int i = shutdownList.Count - 1; i >= 0; i--)
				{
					shutdownList[i].OnDeactivate();
				}
			});

			this.active = false;
		}
		/// <summary>
		/// Updates the Scene.
		/// </summary>
		public void Update()
		{
			if (!this.active) throw new InvalidOperationException("Cannot update a scene that is not active. Call Activate first.");
			if (this.isUpdating) throw new InvalidOperationException("Can't update a Scene while it is already being updated.");
			this.isUpdating = true;
			try
			{
				//ThreeScene.Fog = new THREE.Scenes.Fog(System.Drawing.Color.FromArgb(255, FogColor.R, FogColor.G, FogColor.B), FogNear, FogFar);
				ThreeScene.Background = new THREE.Math.Color(BackgroundColor.R / 255f, BackgroundColor.G / 255f, BackgroundColor.B / 255f);

				// Remove disposed objects from managers
				this.CleanupDisposedObjects();

				// Update all GameObjects
				Profile.TimeUpdateScene.BeginMeasure();
				DualityApp.EditorGuard(() =>
				{
					this.UpdateComponents<ICmpUpdatable>(cmp => cmp.OnUpdate());
				});
				Profile.TimeUpdateScene.EndMeasure();

				// Update coroutines
				Profile.TimeUpdateCoroutines.BeginMeasure();
				this.coroutineManager.Update();
				Profile.TimeUpdateCoroutines.EndMeasure();
			}
			finally
			{
				this.isUpdating = false;
			}
		}
		/// <summary>
		/// Updates the Scene in the editor.
		/// </summary>
		internal void EditorUpdate()
		{
			if (this.isUpdating) throw new InvalidOperationException("Can't update a Scene while it is already being updated.");
			this.isUpdating = true;
			try
			{
				//ThreeScene.Fog = new THREE.Scenes.Fog(System.Drawing.Color.FromArgb(255, FogColor.R, FogColor.G, FogColor.B), FogNear, FogFar);
				ThreeScene.Background = new THREE.Math.Color(BackgroundColor.R / 255f, BackgroundColor.G / 255f, BackgroundColor.B / 255f);
				Profile.TimeUpdateScene.BeginMeasure();
				DualityApp.EditorGuard(() =>
				{
					this.UpdateComponents<ICmpEditorUpdatable>(cmp => cmp.OnUpdate());
				});
				Profile.TimeUpdateScene.EndMeasure();
			}
			finally
			{
				this.isUpdating = false;
			}
		}
		private void UpdateComponents<T>(Action<T> updateAction) where T : class
		{
			Profile.TimeUpdateSceneComponents.BeginMeasure();

			// Create a sorted list of updatable component types
			this.updateTypeOrder.Clear();
			foreach (var pair in this.componentsByType)
			{
				// Skip Component types that aren't updatable anyway
				Component sampleComponent = pair.Value.Count > 0 ? pair.Value[0] : null;
				if (!(sampleComponent is T))
					continue;

				this.updateTypeOrder.Add(pair.Key.AsType());
			}
			Component.ExecOrder.SortTypes(this.updateTypeOrder, false);

			// Gather a list of updatable Components
			this.updatableComponents.Clear();
			this.updateMap.Clear();
			foreach (Type type in this.updateTypeOrder)
			{
				TypeInfo typeInfo = type.GetTypeInfo();
				List<Component> components = this.componentsByType[typeInfo];
				int oldCount = this.updatableComponents.Count;

				// Collect Components
				this.updatableComponents.Reserve(this.updatableComponents.Count + components.Count);
				for (int i = 0; i < components.Count; i++)
				{
					this.updatableComponents.Add(components[i]);
				}

				// Keep in mind how many Components of each type we have in what order
				if (this.updatableComponents.Count - oldCount > 0)
				{
					this.updateMap.Add(new UpdateEntry
					{
						Type = typeInfo,
						Count = this.updatableComponents.Count - oldCount,
						Profiler = Profile.RequestCounter<TimeCounter>(Profile.TimeUpdateScene.FullName + @"\" + typeInfo.Name)
					});
				}
			}

			// Update all Components. They're still sorted by type.
			{
				int updateMapIndex = -1;
				int updateMapBegin = -1;
				TimeCounter activeProfiler = null;
				Component[] data = this.updatableComponents.Data;
				UpdateEntry[] updateData = this.updateMap.Data;

				for (int i = 0; i < data.Length; i++)
				{
					if (i >= this.updatableComponents.Count) break;

					// Manage profilers per Component type
					if (i == 0 || i - updateMapBegin >= updateData[updateMapIndex].Count)
					{
						// Note:
						// Since we're doing this based on index-count ranges, this needs to be
						// done before skipping inactive Components, so we don't run out of sync.

						updateMapIndex++;
						updateMapBegin = i;

						if (activeProfiler != null)
							activeProfiler.EndMeasure();
						activeProfiler = updateData[updateMapIndex].Profiler;
						activeProfiler.BeginMeasure();
					}
					
					// Skip inactive, disposed and detached Components
					if (!data[i].Active) continue;

					// Invoke the Component's update action
					updateAction(data[i] as T);
				}
				
				if (activeProfiler != null)
					activeProfiler.EndMeasure();
			}

			Profile.TimeUpdateSceneComponents.EndMeasure();
		}
		/// <summary>
		/// Cleanes up scene objects that have been disposed since the scene was last updated.
		/// 
		/// This will invoke <see cref="ICmpInitializable"/> deactivate handlers for objects
		/// where deactivation is still pending.
		/// </summary>
		public void CleanupDisposedObjects()
		{
			this.objectManager.Flush();
			foreach (List<Component> cmpList in this.componentsByType.Values)
				cmpList.RemoveAll(i => i == null || i.Disposed);
		}

		/// <summary>
		/// Applies all <see cref="Duality.Resources.PrefabLink">PrefabLinks</see> contained withing this
		/// Scenes <see cref="GameObject">GameObjects</see>.
		/// </summary>
		public void ApplyPrefabLinks()
		{
			PrefabLink.ApplyAllLinks(this.objectManager.AllObjects);
		}
		/// <summary>
		/// Breaks all <see cref="Duality.Resources.PrefabLink">PrefabLinks</see> contained withing this
		/// Scenes <see cref="GameObject">GameObjects</see>.
		/// </summary>
		public void BreakPrefabLinks()
		{
			foreach (GameObject obj in this.objectManager.AllObjects)
				obj.BreakPrefabLink();
		}

		/// <summary>
		/// Clears the Scene, unregistering all GameObjects. This does not <see cref="GameObject.Dispose">dispose</see> them.
		/// </summary>
		public void Clear()
		{
			this.objectManager.Clear();
		}
		/// <summary>
		/// Appends a cloned version of the specified Scenes contents to this Scene.
		/// </summary>
		/// <param name="scene">The source Scene.</param>
		public void Append(ContentRef<Scene> scene)
		{
			if (!scene.IsAvailable) return;
			this.objectManager.AddObjects(scene.Res.RootObjects.Select(o => o.Clone()));
		}
		/// <summary>
		/// Appends the specified Scene's contents to this Scene and consumes the specified Scene.
		/// </summary>
		/// <param name="scene">The source Scene.</param>
		public void Consume(ContentRef<Scene> scene)
		{
			if (!scene.IsAvailable) return;
			Scene otherScene = scene.Res;
			var otherObj = otherScene.RootObjects.ToArray();
			otherScene.Clear();
			this.objectManager.AddObjects(otherObj);
			otherScene.Dispose();
		}

		/// <summary>
		/// Registers a GameObject and all of its children.
		/// </summary>
		/// <param name="obj"></param>
		public void AddObject(GameObject obj)
		{
			if (obj.Scene != null && obj.Scene != this) obj.Scene.RemoveObject(obj);
			this.objectManager.AddObject(obj);
		}
		/// <summary>
		/// Registers a set of GameObjects and all of their children.
		/// </summary>
		/// <param name="objEnum"></param>
		public void AddObjects(IEnumerable<GameObject> objEnum)
		{
			foreach (GameObject obj in objEnum)
			{
				if (obj.Scene == null || obj.Scene == this) continue;
				obj.Scene.RemoveObject(obj);
			}
			this.objectManager.AddObjects(objEnum);
		}
		/// <summary>
		/// Unregisters a GameObject and all of its children
		/// </summary>
		/// <param name="obj"></param>
		public void RemoveObject(GameObject obj)
		{
			if (obj.Scene != this) return;
			if (obj.Parent != null && obj.Parent.Scene == this)
			{
				obj.Parent = null;
			}
			this.objectManager.RemoveObject(obj);
		}
		/// <summary>
		/// Unregisters a set of GameObjects and all of their children.
		/// </summary>
		/// <param name="objEnum"></param>
		public void RemoveObjects(IEnumerable<GameObject> objEnum)
		{
			objEnum = objEnum.Where(o => o.Scene == this);
			foreach (GameObject obj in objEnum)
			{
				if (obj.Parent == null) continue;
				if (obj.Parent.Scene != this) continue;
				obj.Parent = null;
			}
			this.objectManager.RemoveObjects(objEnum);
		}

		/// <summary>
		/// Finds all GameObjects in the Scene that match the specified name or name path.
		/// </summary>
		/// <param name="name"></param>
		public IEnumerable<GameObject> FindGameObjects(string name)
		{
			return this.AllObjects.ByName(name);
		}
		/// <summary>
		/// Finds all GameObjects in the Scene which have a Component of the specified type.
		/// </summary>
		public IEnumerable<GameObject> FindGameObjects(Type hasComponentOfType)
		{
			return this.FindComponents(hasComponentOfType).GameObject();
		}
		/// <summary>
		/// Finds all GameObjects in the Scene which have a Component of the specified type.
		/// </summary>
		public IEnumerable<GameObject> FindGameObjects<T>() where T : class
		{
			return this.FindComponents<T>().OfType<Component>().GameObject();
		}
		/// <summary>
		/// Finds all Components of the specified type in this Scene.
		/// </summary>
		public IEnumerable<T> FindComponents<T>() where T : class
		{
			return this.FindComponents(typeof(T)).OfType<T>();
		}
		/// <summary>
		/// Finds all Components of the specified type in this Scene.
		/// </summary>
		public IEnumerable<Component> FindComponents(Type type)
		{
			TypeInfo typeInfo = type.GetTypeInfo();

			// Determine which by-type lists to use
			bool multiple = false;
			List<Component> singleResult = null;
			List<List<Component>> query = null;
			foreach (var pair in this.componentsByType)
			{
				if (pair.Value.Count == 0) continue;
				if (typeInfo.IsAssignableFrom(pair.Key))
				{
					if (!multiple && singleResult == null)
					{
						// Select single result
						singleResult = pair.Value;
					}
					else
					{
						// Switch to multiselect mode
						if (!multiple)
						{
							query = new List<List<Component>>(this.componentsByType.Values.Count);
							if (singleResult != null) query.Add(singleResult);
						}
						query.Add(pair.Value);
						multiple = true;
					}
				}
			}

			// Found only one match? Return that one.
			IEnumerable<Component> result = null;
			if (!multiple)
			{
				result = singleResult as IEnumerable<Component> ?? new Component[0];
			}
			// Select from a multitude of results
			else
			{
				Component.ExecOrder.SortTypedItems(query, list => list[0].GetType(), false);
				result = query.SelectMany(cmpArr => cmpArr);
			}

			return result;
		}
		
		/// <summary>
		/// Finds a single GameObjects in the Scene that match the specified name or name path.
		/// </summary>
		public GameObject FindGameObject(string name, bool activeOnly = true)
		{
			return (activeOnly ? this.ActiveObjects : this.AllObjects).ByName(name).FirstOrDefault();
		}
		/// <summary>
		/// Finds a single GameObject in the Scene that has a Component of the specified type.
		/// </summary>
		public GameObject FindGameObject(Type hasComponentOfType, bool activeOnly = true)
		{
			Component cmp = this.FindComponent(hasComponentOfType, activeOnly);
			return cmp != null ? cmp.GameObj : null;
		}
		/// <summary>
		/// Finds a single GameObject in the Scene that has a Component of the specified type.
		/// </summary>
		public GameObject FindGameObject<T>(bool activeOnly = true) where T : class
		{
			Component cmp = this.FindComponent<T>(activeOnly) as Component;
			return cmp != null ? cmp.GameObj : null;
		}
		/// <summary>
		/// Finds a single Component of the specified type in this Scene.
		/// </summary>
		public T FindComponent<T>(bool activeOnly = true) where T : class
		{
			return this.FindComponent(typeof(T), activeOnly) as T;
		}
		/// <summary>
		/// Finds a single Component of the specified type in this Scene.
		/// </summary>
		public Component FindComponent(Type type, bool activeOnly = true)
		{
			TypeInfo typeInfo = type.GetTypeInfo();
			foreach (var pair in this.componentsByType)
			{
				if (typeInfo.IsAssignableFrom(pair.Key))
				{
					if (activeOnly)
					{
						foreach (Component cmp in pair.Value)
						{
							if (!cmp.Active) continue;
							return cmp;
						}
					}
					else if (pair.Value.Count > 0)
					{
						return pair.Value[0];
					}
				}
			}

			return null;
		}

		private void AddToManagers(GameObject obj)
		{
			foreach (Component cmp in obj.Components)
				this.AddToManagers(cmp);
		}
		private void AddToManagers(Component cmp)
		{
			// Per-Type lists
			TypeInfo cmpType = cmp.GetType().GetTypeInfo();
			List<Component> cmpList;
			if (!this.componentsByType.TryGetValue(cmpType, out cmpList))
			{
				cmpList = new List<Component>();
				this.componentsByType[cmpType] = cmpList;
			}
			cmpList.Add(cmp);
		}
		private void RemoveFromManagers(GameObject obj)
		{
			foreach (Component cmp in obj.Components)
				this.RemoveFromManagers(cmp);
		}
		private void RemoveFromManagers(Component cmp)
		{
			// Per-Type lists
			TypeInfo cmpType = cmp.GetType().GetTypeInfo();
			List<Component> cmpList;
			if (this.componentsByType.TryGetValue(cmpType, out cmpList))
				cmpList.Remove(cmp);
		}
		private void RegisterManagerEvents()
		{
			this.objectManager.GameObjectsAdded   += this.objectManager_GameObjectsAdded;
			this.objectManager.GameObjectsRemoved += this.objectManager_GameObjectsRemoved;
			this.objectManager.ParentChanged      += this.objectManager_ParentChanged;
			this.objectManager.ComponentAdded     += this.objectManager_ComponentAdded;
			this.objectManager.ComponentRemoving  += this.objectManager_ComponentRemoving;
		}
		private void UnregisterManagerEvents()
		{
			this.objectManager.GameObjectsAdded   -= this.objectManager_GameObjectsAdded;
			this.objectManager.GameObjectsRemoved -= this.objectManager_GameObjectsRemoved;
			this.objectManager.ParentChanged      -= this.objectManager_ParentChanged;
			this.objectManager.ComponentAdded     -= this.objectManager_ComponentAdded;
			this.objectManager.ComponentRemoving  -= this.objectManager_ComponentRemoving;
		}
		
		private void objectManager_GameObjectsAdded(object sender, GameObjectGroupEventArgs e)
		{
			foreach (GameObject obj in e.Objects)
			{
				this.AddToManagers(obj);
				obj.Scene = this;
			}

			// If the scene is active, activate any added objects
			if (this.active)
			{
				// Gather a list of components to activate
				int objCount = 0;
				List<ICmpInitializable> initList = new List<ICmpInitializable>();
				foreach (GameObject obj in e.Objects)
				{
					if (!obj.ActiveSingle) continue;
					obj.GatherInitComponents(initList, false);
					objCount++;
				}

				// If we collected components from more than one object, sort by exec order.
				// Otherwise, we can safely assume that the list is already sorted.
				if (objCount > 1) Component.ExecOrder.SortTypedItems(initList, item => item.GetType(), false);

				// Invoke the init event on all gathered components in the right order
				foreach (ICmpInitializable component in initList)
					component.OnActivate();
			}

			// Fire global event for current main scene
			if (this.IsCurrent && GameObjectsAdded != null)
				GameObjectsAdded(current, e);
		}
		private void objectManager_GameObjectsRemoved(object sender, GameObjectGroupEventArgs e)
		{
			foreach (GameObject obj in e.Objects)
			{
				this.RemoveFromManagers(obj);
				obj.Scene = null;
			}

			// Fire global event for current main scene
			if (this.IsCurrent && GameObjectsRemoved != null)
				GameObjectsRemoved(current, e);

			// If the scene is active, deactivate any removed objects
			if (this.active)
			{
				// Gather a list of components to deactivate
				int objCount = 0;
				List<ICmpInitializable> initList = new List<ICmpInitializable>();
				foreach (GameObject obj in e.Objects)
				{
					if (!obj.ActiveSingle && !obj.Disposed) continue;
					obj.GatherInitComponents(initList, false);
					objCount++;
				}

				// If we collected components from more than one object, sort by exec order.
				// Otherwise, we can safely assume that the list is already sorted.
				if (objCount > 1)
					Component.ExecOrder.SortTypedItems(initList, item => item.GetType(), true);
				else
					initList.Reverse();

				// Invoke the init event on all gathered components in the right order
				foreach (ICmpInitializable component in initList)
					component.OnDeactivate();
			}
		}
		private void objectManager_ParentChanged(object sender, GameObjectParentChangedEventArgs e)
		{
			// Fire global event for current main scene
			if (this.IsCurrent && GameObjectParentChanged != null)
				GameObjectParentChanged(current, e);
		}
		private void objectManager_ComponentAdded(object sender, ComponentEventArgs e)
		{
			this.AddToManagers(e.Component);

			// If the scene is active, activate any added components
			if (this.active && e.Component.Active)
			{
				ICmpInitializable cInit = e.Component as ICmpInitializable;
				if (cInit != null) cInit.OnActivate();
			}

			// Fire global event for current main scene
			if (this.IsCurrent && ComponentAdded != null)
				ComponentAdded(current, e);
		}
		private void objectManager_ComponentRemoving(object sender, ComponentEventArgs e)
		{
			this.RemoveFromManagers(e.Component);

			// If the scene is active, deactivate any removed components
			if (this.active && e.Component.Active)
			{
				ICmpInitializable cInit = e.Component as ICmpInitializable;
				if (cInit != null) cInit.OnDeactivate();
			}

			// Fire global event for current main scene
			if (this.IsCurrent && ComponentRemoving != null)
				ComponentRemoving(current, e);
		}

		protected override void OnSaving(string saveAsPath)
		{
			base.OnSaving(saveAsPath);

			// Prepare all components for saving in reverse order, sorted by type
			List<ICmpSerializeListener> initList = this.FindComponents<ICmpSerializeListener>().ToList();
			for (int i = initList.Count - 1; i >= 0; i--)
				initList[i].OnSaving();

			this.serializeObj = this.objectManager.AllObjects.ToArray();
			this.serializeObj.StableSort(SerializeGameObjectComparison);
		}
		protected override void OnSaved(string saveAsPath)
		{
			if (this.serializeObj != null)
				this.serializeObj = null;

			base.OnSaved(saveAsPath);
			
			// Re-initialize all components after saving, sorted by type
			List<ICmpSerializeListener> initList = this.FindComponents<ICmpSerializeListener>().ToList();
			for (int i = 0; i < initList.Count; i++)
				initList[i].OnSaved();

			// If this Scene is the current one, but it wasn't saved before, update the current Scenes internal ContentRef
			if (this.IsCurrent && current.IsRuntimeResource)
				current = new ContentRef<Scene>(this, saveAsPath);
		}
		protected override void OnLoaded()
		{
			if (this.serializeObj != null)
			{
				this.UnregisterManagerEvents();
				foreach (GameObject obj in this.serializeObj)
				{
					obj.EnsureConsistentData();
					obj.EnsureComponentOrder();
				}
				foreach (GameObject obj in this.serializeObj)
				{
					obj.Scene = this;
					this.objectManager.AddObject(obj);
					this.AddToManagers(obj);
				}
				this.RegisterManagerEvents();
				this.serializeObj = null;
			}

			base.OnLoaded();

			this.ApplyPrefabLinks();
			
			// Initialize all loaded components, sorted by type
			List<ICmpSerializeListener> initList = this.FindComponents<ICmpSerializeListener>().ToList();
			for (int i = 0; i < initList.Count; i++)
				initList[i].OnLoaded();
		}
		protected override void OnDisposing(bool manually)
		{
			base.OnDisposing(manually);

			// If the scene is current, leave it
			if (current.ResWeak == this)
				Current = null;

			// If the scene is otherwise active, deactivate it
			if (this.active)
				this.Deactivate();

			GameObject[] obj = this.objectManager.AllObjects.ToArray();
			this.objectManager.Clear();
			foreach (GameObject g in obj) g.DisposeLater();
		}

		private static int SerializeGameObjectComparison(GameObject a, GameObject b)
		{
			int depthA = 0;
			int depthB = 0;
			while (a.Parent != null)
			{
				a = a.Parent;
				++depthA;
			}
			while (b.Parent != null)
			{
				b = b.Parent;
				++depthB;
			}
			return depthA - depthB;
		}
	}
}
