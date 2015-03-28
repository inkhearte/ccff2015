using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SynchronizerData;
using System;

public class Board : MonoBehaviour {

	[Range(1, 10)]
	public int rows, columns, criticalColumn;

	public Cell cellPrefab;

	public CriticalColumnCell ccPrefab;

	private List<RowData> rowList, movableRowList;

	private static Board instance = null;

	private int adjustedCriticalColumn;

	public int MatchRequirement = 3;

	private int matches = 0;

	public bool autoClearOnMatch = false;

	public int numOfBeatsToSkipAfterMatch = 0;

	private int beatsToSkip = 0;

	public bool rowMoveOnBeat = false;

	public RowPickerData picker;

	private void Awake()
	{
		if(instance == null)
		{
			instance = this;

			adjustedCriticalColumn = criticalColumn - 1;

			rowList = new List<RowData>();

			PerformRowAction (MakeRow);

			movableRowList = new List<RowData>(rowList);

			PerformRowAction (MakeCriticalColumn);
		}
		else
		{
			this.enabled = false;

			Debug.LogWarningFormat ("Duplicate Board instance, {0] disabled.", name);
		}
	}

	private void MakeRow(int x)
	{
		rowList.Add (new RowData(transform, new GameObject().transform, cellPrefab.Size));

		rowList[x].MoveForward (cellPrefab);

		rowList[x].onMoveBackward.AddListener (AddToMovableRowList);
	}

	private void MakeCriticalColumn(int x)
	{
		CriticalColumnCell c = Instantiate<CriticalColumnCell>(ccPrefab);

		c.transform.parent = transform.parent;

		Vector2 scale = cellPrefab.Size * 0.5f;

		c.transform.localScale = new Vector3(scale.x, scale.y, c.transform.localScale.z);

		c.name = string.Format ("_CriticalColumnCell[{0},{1}]", adjustedCriticalColumn, x);

		c.transform.localPosition = new Vector3((cellPrefab.Size.x * adjustedCriticalColumn) + transform.localPosition.x, transform.localPosition.y - (cellPrefab.Size.y * x), transform.localPosition.z);

		c.matchReq = MatchRequirement;

		c.instantClear = autoClearOnMatch;
	}

	private void PerformColumnAction(Action<int> a)
	{
		for(int y = 0; y < columns; y++)
		{
			a.Invoke (y);
		}
	}

	private void PerformRowAction(Action<int> a)
	{
		for(int x = 0; x < rows; x++)
		{
			a.Invoke (x);
		}
	}

	private void Perform2DArrayAction(Action<int, int> a)
	{
		for(int y = 0; y < rows; y++)
		{
			for(int x = 0; x < columns; x++)
			{
				a.Invoke (x, y);
			}
		}
	}

	private void OnDrawGizmos()
	{
		if(cellPrefab != null)
		{
			Perform2DArrayAction (DrawWireCube);
		}	

		if(ccPrefab != null)
		{
			if(autoClearOnMatch)
			{
				Gizmos.color = Color.cyan;
			}
			else
			{
				Gizmos.color = Color.yellow;
			}

			PerformRowAction(DrawWireCube);
		}
	}

	private void DrawWireCube(int x, int y)
	{
		if(x == 0)
		{
			Gizmos.color = Color.green;
		}
		else if (x == columns - 1)
		{
			Gizmos.color = Color.red;
		}
		else
		{
			Gizmos.color = Color.white;
		}
		
		Gizmos.DrawWireCube(new Vector3(transform.position.x + (x * cellPrefab.Size.x), transform.position.y - (y * cellPrefab.Size.y), 0), cellPrefab.Size);
	}

	private void DrawWireCube(int y)
	{
		Vector2 scale = cellPrefab.Size * 0.5f;

		Gizmos.DrawWireCube(new Vector3(transform.position.x + ((criticalColumn - 1) * cellPrefab.Size.x), transform.position.y - (y * cellPrefab.Size.y), 0), scale);
	}

	public void DoOnBeat(BeatType mask)
	{
		if(movableRowList.Count > 0 && mask.Equals (BeatType.OnBeat))
		{
			if(beatsToSkip > 0)
			{
				beatsToSkip--;
			}
			else
			{
				int y = picker.PickRow<RowData> (rowList, movableRowList);
					
				movableRowList[y].MoveForward(cellPrefab);

				if(movableRowList[y].CellCount >= columns && movableRowList[y].Trans.localPosition.x >= columns * cellPrefab.Size.x)
				{
					movableRowList.Remove (movableRowList[y]);
				}
			}
		}
	}

	private void AddToMovableRowList(RowData r)
	{
		if(!movableRowList.Contains (r))
		{
			movableRowList.Add (r);
		}
	}

	public void Match()
	{
		matches = 0;

		PerformRowAction (CheckCriticalColumn);

		if(matches >= MatchRequirement || autoClearOnMatch)
		{
			PerformRowAction (ScoreMatches);

			beatsToSkip = numOfBeatsToSkipAfterMatch;
		}
	}

	private void CheckCriticalColumn(int x)
	{
		if(adjustedCriticalColumn < rowList[x].CellCount && rowList[x].Cells[adjustedCriticalColumn].matched)
		{
			matches++;
		}
	}

	private void ScoreMatches(int x)
	{
		Cell c = rowList[x].RemoveCell(adjustedCriticalColumn);

		if(c != null)
		{
			c.Destroy();
		}
	}

	public static Board Instance
	{
		get
		{
			return instance;
		}
	}

	public List<RowData> RowList
	{
		get
		{
			return rowList;
		}
	}
}
