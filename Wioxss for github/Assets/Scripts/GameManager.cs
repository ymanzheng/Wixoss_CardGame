﻿using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public int Rounds;
        public UILabel Reporting;
        public MyRpc MyRpc;
        public static bool BLocalRound;

        public GameObject TwoDui;
        public GameObject ThreeDui;

        #region 主要关联
        public CreateHands CreateHands;
        public LifeCloth LifeCloth;
        public CardInfo CardInfo;
        public Lrig Lrig;
        public WordInfo WordInfo;
        public SetSigni SetSigni;
        public EnerManager EnerManager;
        public Trash Trash;
        public Check Check;
        #endregion
        public enum GameState
        {
            /// <summary>
            /// 抽5卡,换卡,放置血量,放置0级分身
            /// </summary>
            准备阶段,
            /// <summary>
            /// 被冰冻的精灵在下个竖置阶段不能竖置
            /// </summary>
            竖置阶段,
            /// <summary>
            /// 先攻第一回合抽一张卡,其余抽2卡
            /// </summary>
            抽牌阶段,
            /// <summary>
            /// 只能把场上或者手牌上的一张卡充能,也可以跳过充能阶段
            /// </summary>
            充能阶段,
            /// <summary>
            /// 满足条件,支付足够费用后分身可以成长,或者跳过成长阶段
            /// </summary>
            成长阶段,
            /// <summary>
            /// 能使用魔法卡,技艺卡,精灵出场,发动效果等,要手动结束
            /// </summary>
            主要阶段,
            /// <summary>
            /// 做出攻击宣言,双方能使用[攻击时点]的技艺卡或魔法卡
            /// </summary>
            攻击宣言阶段,
            /// <summary>
            /// 精灵间的战斗,力量大于或等于其正对面的精灵时,可以把对方的精灵驱逐(放置到能量区),若正面没有精灵,则可以对对方造成伤害
            /// </summary>
            精灵攻击阶段,
            /// <summary>
            /// 当所有精灵都攻击结束后,分身发动攻击
            /// </summary>
            分身攻击阶段,
            /// <summary>
            /// 自己回合结束,若手牌大于6张时,要丢弃到6张,交换回合
            /// </summary>
            结束阶段,
            对方回合阶段,
            其他阶段,
        }

        public enum Timing
        {
            主要阶段,
            攻击宣言阶段,
            魔法切入阶段,
            其他阶段,
        }

        public Timing MyTiming;

        private GameState _myGameState = GameState.其他阶段;

        public GameState MyGameState
        {
            get { return _myGameState; }
            set
            {
                _myGameState = value;
                switch (value)
                {
                    case GameState.准备阶段:
                        StartCoroutine(ReadyPhase());
                        break;
                    case GameState.竖置阶段:
                        if (BLocalRound)
                            StartCoroutine(SetPhase());
                        break;
                    case GameState.抽牌阶段:
                        if (BLocalRound)
                            StartCoroutine(DropPhase());
                        break;
                    case GameState.充能阶段:
                        if (BLocalRound)
                            EnerPhase();
                        break;
                    case GameState.成长阶段:
                        if (BLocalRound)
                            GrowPhase();
                        break;
                    case GameState.主要阶段:
                        if (BLocalRound)
                            MainPhase();
                        break;
                    case GameState.攻击宣言阶段:
                        if (BLocalRound)
                            AttackSayPhase();
                        break;
                    case GameState.精灵攻击阶段:
                        if (BLocalRound)
                            SigniAttack();
                        break;
                    case GameState.分身攻击阶段:
                        if (BLocalRound)
                            LrigAttack();
                        break;
                    case GameState.结束阶段:
                        if (BLocalRound)
                            EndPhase();
                        break;
                    case GameState.对方回合阶段:
                        NotMyRound();
                        break;
                    case GameState.其他阶段:
                        break;
                }
            }
        }

        #region ReadyPhase

        private IEnumerator ReadyPhase()
        {
            MyRpc = GameObject.Find("RPC").GetComponent<MyRpc>();

            SetEnableOrDisable(TwoDui, ThreeDui);

            DropFiveCards();
            yield return new WaitForSeconds(2);
            ChangeCards();
            //            yield return new WaitForSeconds(2);
            //            SetLifeCloth();
            //            yield return new WaitForSeconds(2);
            //            SetLrig();
            //            yield return new WaitForSeconds(2);
            //            GameStart();
        }

        private void DropFiveCards()
        {
            StartCoroutine(CreateHands.Setup());
            MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
        }

        private void ChangeCards()
        {
            CreateHands.ShowChangeBtn(true);
        }

        public void RpcChangeCards(int num)
        {
            Reporting.text = "换" + num + "张卡";
            MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
            StartCoroutine(SetLifeCloth());
        }

        private IEnumerator SetLifeCloth()
        {
            Reporting.text = "放置7张生命护甲";
            LifeCloth.CreateLifeCloth();
            MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
            yield return new WaitForSeconds(2);
            SetLrig();
        }

        /// <summary>
        /// 告诉对方我已经设置好护甲
        /// </summary>
        /// <param name="num"></param>
        public void RpcCreateLifeCloth(int num)
        {
            MyRpc.Rpc("SetOtherCloth", RPCMode.Others,num);
        }

        private void SetLrig()
        {
            Reporting.text = "放置0级分身";

            var lrig = new List<Card>();
            for (int i = 0; i < DataSource.LrigDeck.Count; i++)
            {
                if (DataSource.LrigDeck[i].MyCardType == Card.CardType.分身卡)
                {
                    if (DataSource.LrigDeck[i].Level == 0)
                    {
                        lrig.Add(DataSource.LrigDeck[i]);
                    }
                }
            }

            CardInfo.SetUp("放置0级分身", lrig, 1, SelectLrigDelegate);
            CardInfo.ShowCardInfo(true);
            Lrig.SetShowLrigDeck();
            MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);

        }

        private void SelectLrigDelegate()
        {
            Card card;
            for (int i = 0; i < CardInfo.SelectHands.Count; i++)
            {
                card = CardInfo.SelectHands[i].MyCard;
                Lrig.SetUp(card);
                Lrig.ShowLrig(true);

                RpcOtherLrig(card.CardId);

                DataSource.LrigDeck.Remove(card);
                CardInfo.ShowCardInfo(false);

                if (Network.isClient)
                {
                    DataSource.ClientPlayer.BReadyPhaseEnd = true;
                    MyRpc.Rpc("ReportClientEndReadyPhase", RPCMode.Others, true);
                }
                else if (Network.isServer)
                {
                    DataSource.ServerPlayer.BReadyPhaseEnd = true;
                    MyRpc.Rpc("ReportServerEndReadyPhase", RPCMode.Others, true);
                }

                StartCoroutine(WaitToGameStart());
                //GameTest();
            }
        }

        private IEnumerator WaitToGameStart()
        {
            while (true)
            {
                if (DataSource.ClientPlayer.BReadyPhaseEnd && DataSource.ServerPlayer.BReadyPhaseEnd)
                {
                    GameStart();
                    yield break;
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void GameStart()
        {
            Reporting.text = "游戏正式开始";
            MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
            Rounds++;

            SetMyRound();
            BLocalRound = IsMyRound();
            if (!BLocalRound)
            {
                MyGameState = GameState.对方回合阶段;
            }
            MyGameState = GameState.竖置阶段;
        }

        private void SetMyRound()
        {
            if (Network.isClient)
            {
                DataSource.ClientPlayer.BRound = DataSource.ClientPlayer.BFirst;
            }
            if (Network.isServer)
            {
                DataSource.ServerPlayer.BRound = DataSource.ServerPlayer.BFirst;
            }
        }

        #endregion

        private bool IsMyRound()
        {
            if (Network.isClient)
            {
                return DataSource.ClientPlayer.BRound;
            }
            if (Network.isServer)
            {
                return DataSource.ServerPlayer.BRound;
            }
            return false;
        }

        #region SetPhase

        private IEnumerator SetPhase()
        {
            SetAllSigni();
            yield return new WaitForSeconds(2);
            MyGameState = GameState.抽牌阶段;
        }

        private void SetAllSigni()
        {
            //if(Signi!=freezd)
            Lrig.ResetLrig();
            SetSigni.ResetSigni();
            Reporting.text = "竖置所有精灵";
            MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
        }

        #endregion

        #region DropPhase

        private IEnumerator DropPhase()
        {
            DropCard();
            yield return new WaitForSeconds(2);
            MyGameState = GameState.充能阶段;
        }

        private void DropCard()
        {
            if (Rounds == 1)
            {
                Reporting.text = "抽1卡";
                StartCoroutine(CreateHands.DropCard(1));
                MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
            }
            else
            {
                Reporting.text = "抽2卡";
                StartCoroutine(CreateHands.DropCard(2));
                MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
            }
        }

        #endregion

        #region EnerPhase

        private void EnerPhase()
        {
            EnerCharge();
        }

        private void EnerCharge()
        {
            CreateHands.SetEnerChange();
            CreateHands.ShowChargeBtn(true);
            SetSigni.ShowChargeBtn();
        }

        public void RpcEnerCharge()
        {
            CreateHands.ShowChargeBtn(false);
            CreateHands.Reflash();
            SetSigni.DisAllChargeBtn();

            Reporting.text = "从场上或者手牌上选择一张充能";
            MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
            MyGameState = GameState.成长阶段;
        }

        /// <summary>
        /// 告诉别人自己充能了
        /// </summary>
        /// <param name="cardid"></param>
        public void RpcEnerCharge(string cardid)
        {
            MyRpc.Rpc("SetOtherEner", RPCMode.Others, cardid);
        }

        #endregion

        #region GrowPhase

        private void GrowPhase()
        {
            Grow();
        }

        private void Grow()
        {
            //if(Lrig.level = target.level - 1 && Ener>=Lrig.cost)

            Lrig.ShowUpBtn(true);
        }

        public void RpcGrow()
        {
            Lrig.ShowUpBtn(false);
            Reporting.text = "从能量区扣除相应的费用,且精灵等级在规定等级内成长";
            MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
            MyGameState = GameState.主要阶段;
        }

        /// <summary>
        /// 告诉对方我已经放置好了分身
        /// </summary>
        /// <param name="cardid"></param>
        public void RpcOtherLrig(string cardid)
        {
            MyRpc.Rpc("SetOtherLrig", RPCMode.Others, cardid);
        }

        /// <summary>
        ///-1是其他情况把牌放置到弃置区中,例如从能量到废置,0,1,2是指场上的精灵,对应1,2,3位置
        /// </summary>
        /// <param name="cardid">Cardid.</param>
        /// <param name="i">The index.</param>
        public void RpcOtherTrash(string cardid, int i = -1)
        {
            MyRpc.Rpc("SetOtherTrash", RPCMode.Others, cardid, i);
        }

        /// <summary>
        /// 告诉对方自己用了能量
        /// </summary>
        /// <param name="cardid"></param>
        public void RpcDeleteOtherEner(string cardid)
        {
            MyRpc.Rpc("DeleteOtherEner", RPCMode.Others, cardid);
        }

        /// <summary>
        ///告诉对方自己删除了手牌
        /// </summary>
        /// <param name="num">Number.</param>
        /// <param name="bhandkill">If set to <c>true</c> bhandkill.</param>
        public void RpcDestoryOtherHands(int num)
        {
            MyRpc.Rpc("DestoryOtherHand", RPCMode.Others, num);
        }

        /// <summary>
        /// 告诉对方自己新增了手牌
        /// </summary>
        /// <param name="num"></param>
        public void RpcCreateOtherHands(int num)
        {
            MyRpc.Rpc("CreateOtherHands", RPCMode.Others, num);
        }

        #endregion

        #region MainPhase

        private void MainPhase()
        {
            MyMain();
            WordInfo.SetTheEndPhase(() =>
            {
                CreateHands.DisTheUseBtn();
                if(Rounds==1)
                {
                    MyGameState = GameState.结束阶段;
                    WordInfo.ShowTheEndPhaseBtn(false);
                }else
                {
                    MyGameState = GameState.攻击宣言阶段;
                }
            });
            WordInfo.ShowTheEndPhaseBtn(true);
        }

        private void MyMain()
        {
            CreateHands.ShowTheUseBtn();
            //CanUseMagic = true
            //CanUseEffect = true
            //CanUseSigni = true
            MyTiming = Timing.主要阶段;
            Reporting.text = "主要阶段,可以使用魔法卡,可以发动效果,可以出怪";
            MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
            //if(使用魔法)
            //MyTiming = Timing.魔法切入阶段
        }

        /// <summary>
        /// 设置了精灵
        /// </summary>
        /// <param name="num">位置</param>
        /// <param name="cardid">卡牌id</param>
        public void RpcSetSigni(int num, string cardid)
        {
            MyRpc.Rpc("SetOtherSigni", RPCMode.Others, num, cardid);
        }

        /// <summary>
        /// 在检查区中显示
        /// </summary>
        /// <param name="cardid"></param>
        public void RpcCheck(string cardid)
        {
            MyRpc.Rpc("SetOtherCheck", RPCMode.Others, cardid);
        }

        #endregion

        #region AttackPhase

        private void AttackSayPhase()
        {
//            if (Rounds == 1)
//            {
//                Reporting.text = "先手跳过攻击阶段";
//                MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
//                yield return new WaitForSeconds(1);
//                MyGameState = GameState.结束阶段;
//            }
//            else
//            {
                AttackSay();
                //yield return new WaitForSeconds(1);
                MyGameState = GameState.精灵攻击阶段;
                //                yield return new WaitForSeconds(1);
                //                MyGameState = GameState.分身攻击阶段;
                //                yield return new WaitForSeconds(1);
                //                MyGameState = GameState.结束阶段;
            //}
        }

        private void AttackSay()
        {
            MyTiming = Timing.攻击宣言阶段;
            //可以使用该时点的魔法卡,效果等
            Reporting.text = "攻击宣言阶段";
            MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
        }

        private void SigniAttack()
        {
            SetSigni.ShowAttackBtn();

            WordInfo.SetTheEndPhase(() =>
            {
                SetSigni.DisAllAttackBtn();
                MyGameState = GameState.分身攻击阶段;
            });
            WordInfo.ShowTheEndPhaseBtn(true);

            MyTiming = Timing.其他阶段;
            Reporting.text = "精灵攻击阶段";
            MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
        }

        /// <summary>
        /// 对面使我方的牌驱逐
        /// </summary>
        /// <param name="num"></param>
        public void RpcBanish(int num)
        {
            MyRpc.Rpc("SetBanish", RPCMode.Others, num);
        }

        /// <summary>
        /// 告诉对方对方的护甲被击破
        /// </summary>
        public void RpcCrashOtherLifeCloth()
        {
            MyRpc.Rpc("CrashOtherCloth", RPCMode.Others);
        }

        /// <summary>
        /// 告诉对方是否防御了
        /// </summary>
        /// <param name="bguard"></param>
        public void RpcGuard(int bguard)
        {
            MyRpc.Rpc("SetOtherGuard", RPCMode.Others, bguard);
        }

        /// <summary>
        /// 在对方客户端上现在对面的牌被驱逐了
        /// </summary>
        /// <param name="num"></param>
        public void RpcBanishOther(int num)
        {
            MyRpc.Rpc("SetOtherBanish", RPCMode.Others, num);
        }

        /// <summary>
        /// 告诉对面精灵的横置竖置
        /// </summary>
        /// <param name="num">位置,1,2,3</param>
        /// <param name="bset">是否横,竖置</param>
        public void RpcSet(int num, bool bset)
        {
            MyRpc.Rpc("SetOtherSigniSet", RPCMode.Others, num, bset);
        }

        /// <summary>
        /// 告诉对面分身攻击后横置
        /// </summary>
        /// <param name="bset"></param>
        public void RpcLrigSet(bool bset)
        {
            MyRpc.Rpc("SetOtherLrigSet", RPCMode.Others, bset);
        }

        private void LrigAttack()
        {
            Lrig.ShowAttackBtn(true);

            //MyRpc.Rpc("ShowOtherGuard", RPCMode.Others);

            WordInfo.SetTheEndPhase(() =>
            {
                Lrig.ShowAttackBtn(false);
                MyGameState = GameState.结束阶段;
                WordInfo.ShowTheEndPhaseBtn(false);
            });
            WordInfo.ShowTheEndPhaseBtn(true);

            Reporting.text = "分身攻击阶段";
            MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
            //CanUseDef
        }

        /// <summary>
        /// 询问对面是否需要防御,10秒后放弃防御
        /// </summary>
        public void RpcLrigAttack()
        {
            MyRpc.Rpc("ShowOtherGuard", RPCMode.Others);
        }

        #endregion

        #region EndPhase()

        private void EndPhase()
        {
            //yield return new WaitForSeconds(2);
            End();
        }

        private void End()
        {
            //if(hands.count>6)丢牌
            Reporting.text = "回合结束,控制权转移";
            MyRpc.Rpc("ReportOtherStuff", RPCMode.Others, Reporting.text);
            BLocalRound = !BLocalRound;
            Rounds++;
            MyRpc.Rpc("RoundChange", RPCMode.Others, BLocalRound, Rounds);
        }

        #endregion

        private void NotMyRound()
        {
            //除了魔法切入用魔法切入时点&&攻击阶段
        }

        /// <summary>
        /// 用于设置隐藏或开启的函数
        /// </summary>
        /// <param name="obj1">隐藏</param>
        /// <param name="obj2">开启</param>
        private void SetEnableOrDisable(GameObject obj1, GameObject obj2)
        {
            obj1.SetActive(false);
            obj2.SetActive(true);
        }


























        //测试!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //                private void Start()
        //                {
        //                    MyGameState = GameState.准备阶段;
        //                }
        //        
        //                private void GameTest()
        //                {
        //                    BLocalRound = true;
        //                    MyGameState = GameState.竖置阶段;
        //                }
    }
}
