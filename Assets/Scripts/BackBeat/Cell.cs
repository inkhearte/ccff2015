﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using SynchronizerData;
using System;

public class Cell : MonoBehaviour {

	internal class TransData
	{
		public Mesh mesh;

		public Color color;

		public Vector3 scale;

		public Quaternion rotation;

		public TransData(Vector3 s, Quaternion rot, Mesh m = null, Color c = new Color())
		{
			mesh = m;

			color = c;

			scale = s;

			rotation = rot;
		}
	}

	[Serializable]
	public struct RendererStates
	{
		[SerializeField]
		public Material highlight, normal;
	}

	public Transform graphic;

	public MeshRenderer bg;

	public List<Mesh> shapes;

	public List<Color> colors;

	public RendererStates states;

	public float dragTime = 0.2f;

	private float mouseDownTime = 0.0f;
	
	private bool dragging = false;

	public UnityEvent onClick = new UnityEvent();
	public UnityEvent onDrag = new UnityEvent();

	[HideInInspector]
	public CriticalColumnCell ccCell = null;

	private TransData defaultGraphicTD;

	private Animator anim;

	private BeatObserver beatObs;

	[HideInInspector]
	public bool matched = false;

	private void Awake()
	{
		defaultGraphicTD = new TransData(graphic.transform.localScale, graphic.transform.rotation);

		beatObs = GameObject.FindGameObjectWithTag ("OnBeatObs").GetComponent<BeatObserver>();
		beatObs.beatEvent.AddListener (DoOnBeat);

		anim = GetComponentInChildren<Animator>();

		Randomize();

	}

	private void Randomize()
	{
		defaultGraphicTD.mesh = shapes[UnityEngine.Random.Range(0, shapes.Count)];
		defaultGraphicTD.color = colors[UnityEngine.Random.Range(0, colors.Count)];

		graphic.GetComponent<MeshFilter>().mesh = defaultGraphicTD.mesh;
		graphic.GetComponent<MeshRenderer>().material.color = defaultGraphicTD.color;
	}

	private void OnMousedown()
	{
		mouseDownTime = 0.0f;
	}

	private void OnMouseDrag()
	{
		if(mouseDownTime < dragTime)
		{
			mouseDownTime += Time.deltaTime;
		}
		else
		{
			dragging = true;
		}
	}
	
	private void OnMouseUp()
	{
		if(!dragging)
		{
			onClick.Invoke ();
		}
		else
		{
			onDrag.Invoke ();
		}

		dragging = false;
	}

	public bool Match(Cell otherCell)
	{
		return defaultGraphicTD.color.Equals (otherCell.defaultGraphicTD.color) && defaultGraphicTD.mesh.Equals (otherCell.defaultGraphicTD.mesh);
	}

	public void DoOnBeat(BeatType mask)
	{
		if(matched && mask.Equals (BeatType.OnBeat))
		{
			anim.SetTrigger ("OnBeatTrigger");
		}
	}

	public void Destroy()
	{
		onClick.RemoveAllListeners();
		onDrag.RemoveAllListeners ();

		beatObs.beatEvent.RemoveListener(DoOnBeat);

		anim.StopPlayback();

		gameObject.SetActive (false);
	}

	private void FixedUpdate()
	{
		if(ccCell != null)
		{
			graphic.rotation = ccCell.transform.rotation;

			bg.material = states.highlight;
		}
		else
		{
			if(graphic.rotation != defaultGraphicTD.rotation)
			{
				graphic.rotation = Quaternion.Lerp (graphic.rotation, defaultGraphicTD.rotation, 10.0f * Time.deltaTime);

				bg.material = states.normal;
			}
		}
	}

	public Vector2 Size
	{
		get
		{
			return new Vector2(transform.localScale.x, transform.localScale.y);
		}
	}
}
