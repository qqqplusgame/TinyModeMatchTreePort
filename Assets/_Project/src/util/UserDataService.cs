using UnityEngine;

namespace ProjectM
{
    public class UserDataService
    {
        // static getBestScore(levelID: number): number {
        //     let cookieName = "BestScore" + levelID;
        //     let value = this.getCookie(cookieName);
        //     if (value) {
        //         return Number(value);
        //     }
        //     else {
        //         return 0;
        //     }
        // }
        //
        // static setBestScore(levelID: number, score: number): void {
        //     let cookieName = "BestScore" + levelID;
        //     this.setCookie(cookieName, String(score));
        // }
        //
        // static getSelectedWorldMapIndex(): number {
        //     let cookieName = "SelectedWorldMapIndex";
        //     let value = this.getCookie(cookieName);
        //     if (value) {
        //         return Number(value);
        //     }
        //     else {
        //         return 0;
        //     }
        // }
        //
        // static setSelectedWorldMapIndex(itemIndex: number): void {
        //     let cookieName = "SelectedWorldMapIndex";
        //     this.setCookie(cookieName, String(itemIndex));
        // }
        //
        public static int getLastBeatenLevelID()
        {
            var cookieName = "LastBeatenLevelID";
            var value = PlayerPrefs.GetInt(cookieName, 0);

            return value;
        }
        //
        // static setLastBeatenLevelID(levelID: number): void {
        //     let cookieName = "LastBeatenLevelID";
        //     this.setCookie(cookieName, String(levelID));
        // }
        //
        // static getIsSoundOn(): boolean {
        //     let cookieName = "Sound";
        //     let value = this.getCookie(cookieName);
        //     if (value) {
        //         return this.getCookie(cookieName) == "1" ? true : false;
        //     }
        //     else {
        //         return true;
        //     }
        // }
        //
        // static setIsSoundOn(isSoundOn: boolean): void {
        //     let cookieName = "Sound";
        //     this.setCookie(cookieName, String(isSoundOn ? 1 : 0));
        // }
        //
        // static getIsMusicOn(): boolean {
        //     let cookieName = "Music";
        //     let value = this.getCookie(cookieName);
        //     if (value) {
        //         return this.getCookie(cookieName) == "1" ? true : false;
        //     }
        //     else {
        //         return true;
        //     }
        // }
        //
        // static setIsMusicOn(isMusicOn: boolean): void {
        //     let cookieName = "Music";
        //     this.setCookie(cookieName, String(isMusicOn ? 1 : 0));
        // }
        //
        // static getLanguageID(): string {
        //     let cookieName = "LanguageID";
        //     let value = this.getCookie(cookieName);
        //     if (value) {
        //         return this.getCookie(cookieName);
        //     }
        //     else {
        //         return "en";
        //     }
        // }
        //
        // static setLanguageID(languageID: string): void {
        //     let cookieName = "LanguageID";
        //     this.setCookie(cookieName, languageID);
        // }
        //
        // static getHasSeenCutscene(): boolean {
        //     let cookieName = "CutsceneSeen";
        //     let value = this.getCookie(cookieName);
        //     if (value) {
        //         return this.getCookie(cookieName) == "1" ? true : false;
        //     }
        //     else {
        //         return false;
        //     }
        // }
        //
        // static setHasSeenCutscene(hasSeen: boolean): void {
        //     let cookieName = "CutsceneSeen";
        //     this.setCookie(cookieName, String(hasSeen ? 1 : 0));
        // }
        //
        // static getHasSeenEndCutscene(): boolean {
        //     let cookieName = "EndCutsceneSeen";
        //     let value = this.getCookie(cookieName);
        //     if (value) {
        //         return this.getCookie(cookieName) == "1" ? true : false;
        //     }
        //     else {
        //         return false;
        //     }
        // }
        //
        // static setHasSeenEndCutscene(hasSeen: boolean): void {
        //     let cookieName = "EndCutsceneSeen";
        //     this.setCookie(cookieName, String(hasSeen ? 1 : 0));
        // }
        //
        // static getIsMatchTutorialDone(): boolean {
        //     let cookieName = "MatchTutorialDone";
        //     let value = this.getCookie(cookieName);
        //     if (value) {
        //         return this.getCookie(cookieName) == "1" ? true : false;
        //     }
        //     else {
        //         return false;
        //     }
        // }
        //
        // static setIsMatchTutorialDone(value: boolean): void {
        //     let cookieName = "MatchTutorialDone";
        //     this.setCookie(cookieName, String(value ? 1 : 0));
        // }
        //
        // static getIsEggTutorialDone(): boolean {
        //     let cookieName = "EggTutorialDone";
        //     let value = this.getCookie(cookieName);
        //     if (value) {
        //         return this.getCookie(cookieName) == "1" ? true : false;
        //     }
        //     else {
        //         return false;
        //     }
        // }
        //
        // static setIsEggTutorialDone(value: boolean): void {
        //     let cookieName = "EggTutorialDone";
        //     this.setCookie(cookieName, String(value ? 1 : 0));
        // }
        //
        // static getIsSurvivalTutorialDone(): boolean {
        //     let cookieName = "SurvivalTutorialDone";
        //     let value = this.getCookie(cookieName);
        //     if (value) {
        //         return this.getCookie(cookieName) == "1" ? true : false;
        //     }
        //     else {
        //         return false; 
        //     }
        // }
        //
        // static setIsSurvivalTutorialDone(value: boolean): void {
        //     let cookieName = "SurvivalTutorialDone";
        //     this.setCookie(cookieName, String(value ? 1 : 0));
        // }
    }
}