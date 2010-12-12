/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package lib;

import javax.swing.*;

/**
 *
 * @author Ante
 */

public class MyThread implements Runnable{
    private JTextArea txt;
    private String s;

    public MyThread(JTextArea txt) {
        this.txt = txt;
        this.s = s;
    }
    
    public void run() {
        obaviPosao();
    }
    public void setText(String s)
    {
        this.s=s;
    }
    private void obaviPosao() {
        txt.append("\n" + s);
    }
}
