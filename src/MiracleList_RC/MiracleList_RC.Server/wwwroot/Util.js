window.log = (s) => {
 console.log(s);
};

window.GetConfirmation = (text1, text2) => {
 console.log("GetConfirmation", text1, text2);
     return confirm(text1 + "\n" + text2);
   };