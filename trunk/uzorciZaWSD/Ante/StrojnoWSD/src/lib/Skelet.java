/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package lib;

import weka.classifiers.*;
import weka.core.*;
import java.util.*;
import java.io.*;
import weka.classifiers.evaluation.ConfusionMatrix;
import javax.swing.*;
/**
 *
 * @author Ante
 */
public abstract class Skelet {
    protected int[] windowSizes = {0,1,2,3,4,5,10,25,50};
    protected int numOfWindowsSizes = windowSizes.length;
    protected String[][] dataLocations = new String[numOfWindowsSizes][numOfWindowsSizes];
    protected Instances[][] dataSet = new Instances[numOfWindowsSizes][numOfWindowsSizes];
    protected Classifier[][] classifiers = new Classifier[numOfWindowsSizes][numOfWindowsSizes];
    protected int folds = 5;
    protected Random rand;
    protected double[][][] precision81 = new double[numOfWindowsSizes][numOfWindowsSizes][2];

    protected int[][] randomSequence;

    protected String[] stringClasses = {"1", "2", "3", "4", "5", "6"};
    protected ConfusionMatrix cm =  new ConfusionMatrix(stringClasses);

    protected MyThread job;
    protected Thread thread;

    protected JTextArea txt;
    protected int[] genereteSequence(int length)
    {
        int[] list = new int[length];

        for(int i = 0; i < length; i++)
            list[i] = i;

        for(int i = 0; i < length; i++)
        {
            int swap1 = rand.nextInt(length);
            int swap2 = rand.nextInt(length);
            int b = list[swap1];
            list[swap1] = list[swap2];
            list[swap2] = b;
        }

        return list;
    }

    protected void makeRandomSequence()
    {
        int size = dataSet[0][0].testCV(folds, 0).numInstances() + 2;
        randomSequence = new int[size][folds];

        for(int k = 0; k < folds; k++)
            randomSequence[k] = genereteSequence(dataSet[0][0].testCV(folds, k).numInstances());
    }

    protected int getIndex(int foldK, int i)
    {
        return randomSequence[foldK][i];
    }

    protected void incConfusionMatrix(ConfusionMatrix m, int row, int col)
    {
        double val = m.getElement(row, col) + 1.0;
        m.setElement(row, col, val);
    }

    protected String[] classesToString()
    {
        return stringClasses;
    }

    protected void setDataLocations(String[][] datalocations)
    {
        try
        {
            for(int indexLeft = 0; indexLeft < numOfWindowsSizes; indexLeft++)
            {
                for(int indexRight = 0; indexRight < numOfWindowsSizes; indexRight++)
                {
                    FileReader reader = new FileReader(dataLocations[indexLeft][indexRight]);

                    Instances instances = new Instances(reader);
                    instances.setClassIndex(instances.numAttributes() - 1);

                    //instances.randomize(rand);
                    //cross-validation, podjela na skupove
                    instances.stratify(folds);

                    dataSet[indexLeft][indexRight]  = instances;

                   println("readed data " + windowSizes[indexLeft]+"x"+windowSizes[indexRight]);
                }
            }
        }
        catch(Exception e)
        {
            System.out.println(e.toString());
        }
    }

    public void setTextField(JTextArea txt)
    {
        this.txt = txt;
        job = new MyThread(txt);
        thread = new Thread(job);
    }
    /*
    public void println(String s)
    {

        try{
            job.setText(s);
            thread.start();
            thread.sleep(1000);
            thread.join();
        }
        catch(Exception e)
        {
            e.printStackTrace();
        }
    }
    */
    /*
    void println(final String s)
    {
        try{
            final String[] myStrings = new String[1];

            Runnable setTextFieldText = new Runnable() {
                public void run() {
                    txt.append("\n" + s);
                }
            };

            SwingUtilities.invokeAndWait(setTextFieldText);;
        }
        catch(Exception e)
        {
            e.printStackTrace();
        }
    }
    */

    void println(String s)
    {
        System.out.println("----------------------" + s);
    }
}
