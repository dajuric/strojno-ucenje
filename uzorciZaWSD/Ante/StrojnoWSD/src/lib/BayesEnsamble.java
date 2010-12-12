/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package lib;

import weka.classifiers.*;
import weka.classifiers.bayes.*;
import weka.core.*;
import java.io.*;
import java.text.DecimalFormat;
import java.util.*;
import weka.classifiers.evaluation.ConfusionMatrix;
/**
 *
 * @author Ante
 */
public class BayesEnsamble {
    private int[] windowSizes = {0,1,2,3,4,5,10,25,50};
    private int numOfWindowsSizes = windowSizes.length; 
    private String[][] dataLocations = new String[numOfWindowsSizes][numOfWindowsSizes];
    private Instances[][] dataSet = new Instances[numOfWindowsSizes][numOfWindowsSizes];
    private Classifier[][] classifiers = new Classifier[numOfWindowsSizes][numOfWindowsSizes];
    private int folds = 5;
    private Random rand;
    double[][][] precision81 = new double[numOfWindowsSizes][numOfWindowsSizes][2];

    String[] stringClasses = {"1", "2", "3", "4", "5", "6"};
        ConfusionMatrix cm =  new ConfusionMatrix(stringClasses);
        //testSet[0][0].classAttribute().enumerateValues();

    public BayesEnsamble()
    {
        int seed = new Random().nextInt();
        rand = new Random(seed);   // create seeded number generator
    }
    
    private void setDataLocations(String[][] datalocations)
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

                    Classifier scheme = new NaiveBayes();
                    //scheme.buildClassifier(instances.trainCV(folds, 1));
                    classifiers[indexLeft][indexRight] = scheme;

                    System.out.println("built classifier " + windowSizes[indexLeft]+"x"+windowSizes[indexRight]);
                }
            }
        }
        catch(Exception e)
        {
            System.out.println(e.toString());
        }
    }

    private void train(Instances[][] trainSet)
    {
        for(int indexLeft = 0; indexLeft < numOfWindowsSizes; indexLeft++)
        {
            for(int indexRight = 0; indexRight < numOfWindowsSizes; indexRight++)
            {
                System.out.println("Training " + windowSizes[indexLeft]+"x"+windowSizes[indexRight]);
                Classifier scheme = classifiers[indexLeft][indexRight];
                Instances instances = trainSet[indexLeft][indexRight];
                try
                {
                    scheme.buildClassifier(instances);
                }
                catch(Exception e)
                {
                    System.out.println(e.toString());
                }
            }
        }
    }

    private double getMaxProb(double[] prob)
    {
        double max = prob[0];
        for(int i = 0; i < prob.length; i++)
            if (prob[i] >= max) max = prob[i];

        return max;
    }
    /*
    private void test(Instances[][] testSet, int numOfTestInstances)
    {

        System.out.println(numOfTestInstances);
        int numTP = 0;
        int numFP = 0;

        int numClasses = testSet[0][0].numClasses();
        double[][] results = new double[numOfWindowsSizes][numOfWindowsSizes];

        for(int i = 0; i < numOfTestInstances; i++)
        {
            double[][] ensambleProb = {{0.0, 0.0, 0.0}, {0.0, 0.0, 0.0}, {0.0, 0.0, 0.0}};
            int[][] ensambleID = new int[3][3];
            int[] votes = new int[numClasses];

            for(int indexLeft = 0; indexLeft < numOfWindowsSizes; indexLeft++)
            {
                for(int indexRight = 0; indexRight < numOfWindowsSizes; indexRight++)
                {
                    Classifier scheme = classifiers[indexLeft][indexRight];
                    Instances instances = testSet[indexLeft][indexRight];
                    try
                    {
                        int classID = (int)scheme.classifyInstance(instances.instance(i));
                        double max = getMaxProb(scheme.distributionForInstance(instances.instance(i)));

                        if(max >= ensambleProb[indexLeft/3][indexRight/3])
                        {
                            ensambleProb[indexLeft/3][indexRight/3] = max;
                            ensambleID[indexLeft/3][indexRight/3] = classID;
                        }

                        votes[classID]++;
                        //System.out.println(scheme.distributionForInstance(instances.instance(i)));
                        //System.out.println(instances.testCV(folds, 0).numInstances());
                    }
                    catch(Exception e)
                    {
                        System.out.println(e.toString());
                    }
                }
            }

            /*
            int maxClassVotes = -1;
            int maxClassID = -1;
            int sum = 0;
            for(int k = 0; k < numClasses; k++)
            {
                if(votes[k] > maxClassVotes){
                    maxClassID = k;
                    maxClassVotes = votes[k];
                }
                sum += votes[k];
            }

            int trueClassID = (int)testSet[0][0].instance(i).classValue();
            double val = cm.getElement(trueClassID, maxClassID);
            cm.setElement(trueClassID, maxClassID, val + 1.0);

            if(maxClassID != trueClassID) numFP++;
            else numTP++;
            System.out.println("Result for "+i+" "+ maxClassVotes + "/" + sum +" -- " + maxClassID);
            
            int[] votesEnsamble = new int[numClasses];
            for(int k = 0; k < 3; k++)
            {
                for(int l = 0; l < 3; l++) votesEnsamble[ensambleID[k][l]]++;
            }
            printResult(i, votesEnsamble, testSet[5][5].instance(i));
           
        }

        //System.out.println("TP "+numTP+" numFP"+ numFP);
        System.out.println(cm.toString());
        System.out.println(cm.errorRate());
    }
    */

    private int[] genereteSequence(int length)
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

    private void test(Instances[][] testSet, int numOfTestInstances)
    {
        int[][][] ensamble = new int[3][3][2];
        double[][] maxPrecision = new double[3][3];
        int[] randomList = genereteSequence(testSet[0][0].numInstances());

        //double[][][] precision81 = new double[numOfWindowsSizes][numOfWindowsSizes][2];
        
        try{
            for(int indexLeft = 0; indexLeft < numOfWindowsSizes; indexLeft++)
            {
                for(int indexRight = 0; indexRight < numOfWindowsSizes; indexRight++)
                {
                    int numTP = 0;
                    int numFP = 0;

                    Classifier scheme = classifiers[indexLeft][indexRight];
                    Instances devtest = testSet[indexLeft][indexRight];

                    for(int i = 0; i < devtest.numInstances()/2; i++)
                    {
                        Instance inst = devtest.instance(randomList[i]);
                        int out = (int)scheme.classifyInstance(inst);
                        if(out != inst.classValue()) numFP++;
                        else numTP++;
                    }

                    double precision = (double) numTP /(double)(numTP + numFP);
                    if (precision > maxPrecision[indexLeft/3][indexRight/3])
                    {
                        maxPrecision[indexLeft/3][indexRight/3] = precision;
                        ensamble[indexLeft/3][indexRight/3][0] = indexLeft;
                        ensamble[indexLeft/3][indexRight/3][1] = indexRight;
                    }

                    precision81[indexLeft][indexRight][0] = numTP;
                    precision81[indexLeft][indexRight][1] = numTP + numFP;
                    System.out.println("TP:"+numTP+" FP:"+numFP);
                }
            }

            int num = testSet[0][0].numInstances();
            for(int k = num/2; k < num; k++)
            {
                int[] votes = new int[testSet[0][0].numClasses()];
                int out=-1;
                int trueOut=-1;

                for(int i = 0; i < 3; i++)
                {
                    for(int j = 0; j < 3; j++)
                    {
                        int indexLeft = ensamble[i][j][0];
                        int indexRight = ensamble[i][j][1];
                        Instances devtest = testSet[indexLeft][indexRight];
                        Classifier scheme = classifiers[indexLeft][indexRight];

                        Instance inst = devtest.instance(randomList[k]);
                        out = (int)scheme.classifyInstance(inst);
                        votes[out]++;
                        trueOut = (int) inst.classValue();
                    }
                }

                int max = -1;
                int maxID = -1;

                for(int l = 0; l < testSet[0][0].numClasses(); l++)
                    if(votes[l]>max)
                    {
                        max = votes[l];
                        maxID = l;
                    }

                System.out.println("True: " + trueOut + " Out:" +maxID);
                double val = cm.getElement(trueOut, maxID);
                cm.setElement(trueOut, maxID, val + 1.0);
                //System.out.println("True: " + trueOut + " Out:" +maxID);
            }
        }
        catch(Exception e)
        {
            System.out.println(e.toString());
        }
    }

    private void printResult(int i, int[] votes, Instance instance)
    {
            int maxClassVotes = -1;
            int maxClassID = -1;
            int sum = 0;
            for(int k = 0; k < votes.length; k++)
            {
                if(votes[k] > maxClassVotes){
                    maxClassID = k;
                    maxClassVotes = votes[k];
                }
                sum += votes[k];
            }

            int trueClassID = (int)instance.classValue();
            double val = cm.getElement(trueClassID, maxClassID);
            cm.setElement(trueClassID, maxClassID, val + 1.0);

            if(trueClassID != maxClassID) System.out.print("false - ");
            System.out.println("Result for "+i+" "+ maxClassVotes + "/" + sum +" -- " + maxClassID);
    }

    private void classify(Instances[][] trainSet, Instances[][] testSet)
    {
        train(trainSet);
        test(testSet, testSet[0][0].numInstances());
    }

    private void RandomizeInstances(Instances instances)
    {
        int seed = new Random().nextInt();
        Random rand = new Random(seed);   // create seeded number generator
        instances.randomize(rand);
        instances.stratify(folds);
    }
    public void foo()
    {
        for(int indexLeft = 0; indexLeft < numOfWindowsSizes; indexLeft++)
        {
            for(int indexRight = 0; indexRight < numOfWindowsSizes; indexRight++)
            {
                dataLocations[indexLeft][indexRight] = "D:\\arff\\interest\\interest" +windowSizes[indexLeft]+"x"+windowSizes[indexRight]+".arff";
            }
        }

        setDataLocations(dataLocations);

        for(int i = 0; i < folds; i++){

            System.out.println("FOLD " + (i+1));

            Instances[][] trainSet = new Instances[numOfWindowsSizes][numOfWindowsSizes];
            Instances[][] testSet = new Instances[numOfWindowsSizes][numOfWindowsSizes];

            for(int indexLeft = 0; indexLeft < numOfWindowsSizes; indexLeft++)
            {
                for(int indexRight = 0; indexRight < numOfWindowsSizes; indexRight++)
                {
                    trainSet[indexLeft][indexRight] = dataSet[indexLeft][indexRight].trainCV(folds, i);
                    testSet[indexLeft][indexRight] = dataSet[indexLeft][indexRight].testCV(folds, i);
                    //System.out.println(testSet[indexLeft][indexRight].numInstances());
                }
            }

            classify(trainSet, testSet);
        }

                    DecimalFormat df = new DecimalFormat("#.##");
            for(int i = 0; i <9; i++)
            {
                for(int j = 0; j <9; j++)
                    System.out.print(df.format(precision81[i][j][0] / + precision81[i][j][1]) +"  ");
                System.out.println();
            }

            System.out.println(cm.toString());
        /*
        try{
            FileReader reader = new FileReader(file);
            Instances instances = new Instances(reader);
            instances.setClassIndex(instances.numAttributes() - 1);

            System.out.println(instances.numAttributes());
            System.out.println(instances.numInstances());

            Instance inst = new Instance(instances.numAttributes());
            inst.setValue(instances.attribute("biggest"), 1);
            inst.setValue(instances.attribute("honor"), 1);
            inst.setValue(instances.attribute("a"), 1);
            inst.setValue(instances.attribute("definition"), "interest_1");
            inst.setDataset(instances);

            int seed = 100;
            int folds = 10;

            Classifier scheme = new NaiveBayes();
            scheme.buildClassifier(instances);

            Evaluation eval = new Evaluation(instances);
            eval.crossValidateModel(scheme, instances, 10, new Random(1));
            System.out.println(eval.toSummaryString());
            System.out.println(eval.weightedFMeasure());
*/
            /*
            Random rand = new Random(seed);   // create seeded number generator
            instances.randomize(rand);

            instances.stratify(folds);

             for (int n = 0; n < folds; n++) {
                Instances trainInstances = instances.trainCV(folds, n);
                Instances testInstances = instances.testCV(folds, n);

                 Classifier scheme = new NaiveBayes();
                 scheme.buildClassifier(trainInstances);
                 
                 Evaluation evaluation = new Evaluation(trainInstances);
                 evaluation.evaluateModel(scheme, testInstances);
                 System.out.println(evaluation.toSummaryString());

                System.out.println("built " + n);
             }
            */
            /*
            System.out.println("classifing");
            inst.missingValue();
            double[] d = naiveBayes.distributionForInstance(inst);
            for(int i = 0; i < d.length; i++)
                System.out.println(d[i]);
             * 
             */
/*
        }
        catch(Exception e)
        {
            System.out.println(e.toString());
        }
 * 
 */
    }
}
