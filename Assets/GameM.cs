using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameM : MonoBehaviour
{
    public Transform[] _lineVs;
    public List<Transform> _bags;
    public Camera _mCamera;

    public TrsInfo[] _trsI;

    public float _bagsSpeed;

    public Material _onM, _offM;

    void Start()
    {
        _bags = new List<Transform>();
        _bagTimers[0] = _bagFullTimer * Random.Range(0.10f, 0.70f);
        _bagTimers[1] = _bagFullTimer * Random.Range(0.10f, 0.70f);
        _bagTimers[2] = _bagFullTimer * Random.Range(0.10f, 0.70f);
    }

    public TextMeshProUGUI[] _tLCT;
    public int[] _trueLinesCounter;
    void Update()
    {
        BagGen();

        Ray ray = _mCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && Input.GetMouseButtonDown(0))
            if (hit.transform.GetComponent<TrsInfo>() != null)
            {
                hit.transform.GetComponent<TrsInfo>()._on = true;
                hit.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = _onM;
                hit.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
            }

        foreach (TrsInfo trI in _trsI)
            if (trI._on)
            {
                if (trI._bagI < 0)
                {
                    for (int i = 0; i < _bags.Count; i++)
                    {
                        BagInfo bag = _bags[i].GetComponent<BagInfo>();
                        if (bag._line == trI._sLine && bag._inLinePos - 1 == trI._sLinePos)
                        {
                            bag._onTr = true;
                            trI._bagI = i;
                            break;
                        }
                    }
                }
                else
                {
                    BagInfo bag = _bags[trI._bagI].GetComponent<BagInfo>();

                    Vector3 pos1 = bag.transform.position;
                    Vector3 pos2 = _lineVs[trI._eLine].GetChild(trI._eLinePos).position;

                    if (Vector3.Distance(pos1, pos2) < 0.5f)
                    {
                        bag._onTr = false;
                        bag._line = trI._eLine;
                        bag._inLinePos = trI._eLinePos + 1;
                        trI._bagI = -1;
                        trI._on = false;
                        trI.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = _offM;
                        trI.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);

                    }
                    else
                        bag.transform.position += (pos2 - pos1).normalized * _bagsSpeed * Time.deltaTime;
                }
            }

        foreach (Transform bagX in _bags)
        {
            BagInfo bag = bagX.GetComponent<BagInfo>();
            if (!bag._stop && !bag._onTr)
            {
                Vector3 pos1 = bag.transform.position;
                Vector3 pos2 = _lineVs[bag._line].GetChild(bag._inLinePos).position;

                if (Vector3.Distance(pos1, pos2) < 0.5f)
                    bag._inLinePos++;
                if (bag._inLinePos >= _lineVs[bag._line].childCount)
                {
                    if (bag._line != 3)
                    {
                        bag._stop = true;
                        bag.gameObject.SetActive(false);
                        if(bag._line == bag._trueLine)
                        {
                            _trueLinesCounter[bag._line]--;
                            _tLCT[bag._line].text = 3 - _trueLinesCounter[bag._line] + "/3";
                        }
                    }
                    else
                        bag._inLinePos = 0;
                }
                else
                    bag.transform.position += (pos2 - pos1).normalized * _bagsSpeed * Time.deltaTime;
            }
        }
    }


    public GameObject[] _bagPrefabs;
    public Transform _bagP;
    public int[] _bagCounters;
    public float _bagFullTimer;
    public float[] _bagTimers;
    void BagGen()
    {
        for (int i = 0; i < 3; i++)
        {
            _bagTimers[i] -= Time.deltaTime;

            if (_bagTimers[i] <= 0f && _bagCounters[i] > 0)
            {
                _bagTimers[i] = _bagFullTimer * Random.Range(0.80f, 1.20f);
                _bagCounters[i]--;

                _bags.Add(Instantiate(_bagPrefabs[i], _lineVs[_bagPrefabs[i].GetComponent<BagInfo>()._line].GetChild(0).position, Quaternion.Euler(Vector3.zero), _bagP).transform);
            }

        }
    }

}