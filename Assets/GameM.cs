using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameM : MonoBehaviour
{
    public Transform[] _lineVs;
    public List<Transform> _bags;
    public Camera _mCamera;

    public TrsInfo[] _trsI;

    public float _bagsSpeed;

    public Material _onM, _offM;

    public RectTransform _bag1, _bag2;
    public Vector3 _bag1PosS, _bag1PosE;
    public Vector3 _bag2PosS, _bag2PosE;

    void Start()
    {
        _bags = new List<Transform>();
        _bagTimers[0] = _bagFullTimer * Random.Range(0.10f, 0.30f);
        _bagTimers[1] = _bagFullTimer * Random.Range(0.20f, 0.50f);
    }

    public TextMeshProUGUI[] _tLCT;
    public int[] _trueLinesCounter;

    public AnimationCurve _showEffictCurve;
    public float _effictTimer;
    public Image _showEffictP;

    void Update()
    {
        BagGen();

        _effictTimer += Time.deltaTime;

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

                        _showEffictP.color = Color.red;

                        if (bag._line == bag._trueLine)
                        {
                            _trueLinesCounter[bag._line]--;
                            _tLCT[bag._line].text = 3 - _trueLinesCounter[bag._line] + "/3";

                            if (bag._line == 0)
                                _bag1.gameObject.SetActive(true);
                            else
                                _bag2.gameObject.SetActive(true);

                            _showEffictP.color = Color.green;
                        }

                        _showEffictP.gameObject.SetActive(true);
                        _effictTimer = 0f;
                    }
                    else
                        bag._inLinePos = 0;
                }
                else
                    bag.transform.position += (pos2 - pos1).normalized * _bagsSpeed * Time.deltaTime;
            }
        }

        if (_bag1.gameObject.activeSelf)
        {
            _bag1.localPosition += (_bag1PosE - _bag1.localPosition).normalized * 2300f * Time.deltaTime;
            if (Vector3.Distance(_bag1.localPosition, _bag1PosE) < 30f)
            {
                _bag1.gameObject.SetActive(false);
                _bag1.localPosition = _bag1PosS;
            }
        }
        if (_bag2.gameObject.activeSelf)
        {
            _bag2.localPosition += (_bag2PosE - _bag2.localPosition).normalized * 2300f * Time.deltaTime;
            if (Vector3.Distance(_bag2.localPosition, _bag2PosE) < 30f)
            {
                _bag2.gameObject.SetActive(false);
                _bag2.localPosition = _bag2PosS;
            }
        }

        if (_showEffictP.gameObject.activeSelf)
        {
            Color c = _showEffictP.color;
            c.a = _showEffictCurve.Evaluate(_effictTimer)/3f;
            _showEffictP.color = c;

            if (_effictTimer > 1f)
                _showEffictP.gameObject.SetActive(false);
        }

    }


    public GameObject[] _bagPrefabs;
    public Transform _bagP;
    public int[] _bagCounters;
    public float _bagFullTimer;
    public float[] _bagTimers;
    void BagGen()
    {
        for (int i = 0; i < 2; i++)
        {
            _bagTimers[i] -= Time.deltaTime;

            if (_bagTimers[i] <= 0f && _bagCounters[i] > 0)
            {
                _bagTimers[i] = _bagFullTimer * Random.Range(0.60f, 1.20f);
                _bagCounters[i]--;

                _bags.Add(Instantiate(_bagPrefabs[i], _lineVs[_bagPrefabs[i].GetComponent<BagInfo>()._line].GetChild(0).position, Quaternion.Euler(Vector3.zero), _bagP).transform);
            }

        }
    }

}