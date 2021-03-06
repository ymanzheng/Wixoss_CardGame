﻿using UnityEngine;
using System;

namespace Assets.Scripts
{
    public class MyRpc : MonoBehaviour
    {
        //        /// <summary>
        //        /// 用于传输数字式的数据
        //        /// </summary>
        //        public Action<int> NumChangeAction;
        //        /// <summary>
        //        /// 用于传输字符串的数据
        //        /// </summary>
        //        public Action<string> StringChangeAction;

        private GameManager _gameManager = null;

        private void Awake()
        {
            //保证自己不会在load场景的时候被销毁
            DontDestroyOnLoad(gameObject);
        }

        public void Rpc(string funcname, RPCMode mode, params object[] objs)
        {
            if (Network.peerType != NetworkPeerType.Disconnected)
            {
                networkView.RPC(funcname, mode, objs);
            }
        }

        #region 只能在此定义的蛋疼函数

        [RPC]
        private void ReportClientDeckName(string deckname)
        {
            DataSource.ClientDeckName = deckname;
            DataSource.ClientPlayer.BReady = true;
            var info = GameObject.Find("Info").GetComponent<UILabel>();
            info.text += "\n" + "对方已经准备好了,选择的卡组为: " + deckname;
        }

        [RPC]
        private void ReportServerDeckName(string deckname)
        {
            DataSource.ServerDeckName = deckname;
            DataSource.ServerPlayer.BReady = true;
            var info = GameObject.Find("Info").GetComponent<UILabel>();
            info.text += "\n" + "对方已经准备好了,选择的卡组为: " + deckname;
        }

        [RPC]
        private void ReportClientLoad(bool ready)
        {
            DataSource.ClientPlayer.BLoad = ready;
        }

        [RPC]
        private void ReportServerLoad(bool ready)
        {
            DataSource.ServerPlayer.BLoad = ready;
        }

        [RPC]
        private void ClientSelectFirstAttack(bool bfirst)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            DataSource.ClientPlayer.BFirst = bfirst;
            DataSource.ServerPlayer.BFirst = !bfirst;
            _gameManager.MyGameState = GameManager.GameState.准备阶段;
        }

        [RPC]
        private void ServerSelectFirstAttack(bool bfirst)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            DataSource.ServerPlayer.BFirst = bfirst;
            DataSource.ClientPlayer.BFirst = !bfirst;
            _gameManager.MyGameState = GameManager.GameState.准备阶段;
        }

        [RPC]
        private void ReportClientJyanKen(bool bJyan, int num)
        {
            DataSource.ClientPlayer.BJyanKen = bJyan;
            DataSource.ClientPlayer.JyanKenNum = num;
        }

        [RPC]
        private void ReportServerJyanKen(bool bJyan, int num)
        {
            DataSource.ServerPlayer.BJyanKen = bJyan;
            DataSource.ServerPlayer.JyanKenNum = num;
        }

        [RPC]
        private void ReportClientEndReadyPhase(bool bready)
        {
            DataSource.ClientPlayer.BReadyPhaseEnd = true;
        }

        [RPC]
        private void ReportServerEndReadyPhase(bool bready)
        {
            DataSource.ServerPlayer.BReadyPhaseEnd = true;
        }

        [RPC]
        private void SetOtherSigni(int num, string cardid)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }

            _gameManager.SetSigni.SetOtherSigni(num, new Card(cardid));
        }

        [RPC]
        private void SetOtherSigniSet(int num, bool bset)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }

            _gameManager.SetSigni.SetOtherSigniSet(num, bset);
        }

        [RPC]
        private void SetOtherLrig(string cardid)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _gameManager.Lrig.SetOtherLrig(cardid);
        }

        [RPC]
        private void SetOtherLrigSet(bool bset)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _gameManager.Lrig.SetOtherLrigSet(bset);
        }

        [RPC]
        private void SetOtherEner(string cardid)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _gameManager.EnerManager.CreateOtherEner(cardid);
        }

        [RPC]
        private void SetBanish(int num)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _gameManager.SetSigni.BanishMySigni(num);
        }

        [RPC]
        private void SetOtherBanish(int num)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _gameManager.SetSigni.ShowBanishOtherSigni(num);
        }

        [RPC]
        private void SetOtherTrash(string cardid, int num)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            if (num != -1)
            {
                _gameManager.SetSigni.TrashOtherSigni(num);
            }
            _gameManager.Trash.AddOtherTrash(cardid);
        }

        [RPC]
        private void DestoryOtherHand(int num)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }

            StartCoroutine(_gameManager.CreateHands.DestoryOtherHands(num));

            //_gameManager.CreateHands.DestoryHands(num);
        }

        [RPC]
        private void CreateOtherHands(int num)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _gameManager.CreateHands.CreateOtherHands(num);
        }

        [RPC]
        private void DeleteOtherEner(string cardid)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _gameManager.EnerManager.DestoryOtherEner(cardid);
        }

        [RPC]
        private void SetOtherCheck(string cardid)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _gameManager.Check.SetOtherCheck(cardid);
        }

        [RPC]
        private void SetOtherGuard(int bguard)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _gameManager.Lrig.Bguard = bguard;
        }

        [RPC]
        private void ShowOtherGuard()
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _gameManager.CreateHands.ShowGuardBtn();
        }

        [RPC]
        private void SetOtherCloth(int num)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _gameManager.LifeCloth.CreateOtherCloth(num);
        }

        [RPC]
        private void CrashOtherCloth()
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _gameManager.LifeCloth.CrashCloth();
            _gameManager.CreateHands.DisTheGuardBtn();
        }

//        [RPC]
//        private void CrashCloth()
//        {
//            if (_gameManager == null)
//            {
//                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
//            }
//            _gameManager.LifeCloth.CrashCloth();
//        }

        [RPC]
        private void ReportOtherStuff(string word)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            _gameManager.Reporting.text = "对方回合:" + "\n" + word;
        }

        [RPC]
        private void RoundChange(bool bMyRound, int roundNum)
        {
            if (_gameManager == null)
            {
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            GameManager.BLocalRound = !bMyRound;
            _gameManager.MyGameState = GameManager.GameState.竖置阶段;
            _gameManager.Rounds = roundNum;
        }

        #endregion
    }
}
