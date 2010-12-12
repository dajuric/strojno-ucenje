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
public class BayesEnsamble2 {
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

    public BayesEnsamble2()
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

    private void test() throws Exception
    {
        int size = dataSet[0][0].testCV(folds, 0).numInstances() + 2;
        int[][] randomSequence = new int[size][folds];
        
        for(int k = 0; k < folds; k++)
            randomSequence[k] = genereteSequence(dataSet[0][0].testCV(folds, k).numInstances());

        double[][] acc = new double[numOfWindowsSizes][numOfWindowsSizes];

        for(int indexLeft = 0; indexLeft < numOfWindowsSizes; indexLeft++)
        {
            for(int indexRight = 0; indexRight < numOfWindowsSizes; indexRight++)
            {
                System.out.println("validator classificator: " + windowSizes[indexLeft] + "x" + windowSizes[indexRight]);
                Classifier scheme = classifiers[indexLeft][indexRight];
                Instances data = dataSet[indexLeft][indexRight];

                double correct = 0.0;
                double incorrect = 0.0;

                for(int k = 0; k < folds; k++)
                {
                    Instances trainSet = data.trainCV(folds, k);
                    Instances valTestSet = data.testCV(folds, k);

                    try
                    {
                        scheme.buildClassifier(trainSet);
                        ConfusionMatrix m = validate(scheme, valTestSet, 0, valTestSet.numInstances()/2, randomSequence[k]);
                        correct = m.correct();
                        incorrect = m.incorrect();
                    }
                    catch(Exception e)
                    {
                        e.printStackTrace();
                    }
                }

                acc[indexLeft][indexRight] = correct/(correct + incorrect);
                System.out.println("acc: " + acc[indexLeft][indexRight] );
            }
        }

        //odredi ansambl
        int[][][] ensamble = new int[3][3][2];
        double[][] maxAcc = new double[3][3];

        for(int indexLeft = 0; indexLeft < numOfWindowsSizes; indexLeft++)
        {
            for(int indexRight = 0; indexRight < numOfWindowsSizes; indexRight++)
            {
                if (acc[indexLeft][indexRight] >= maxAcc[indexLeft/3][indexRight/3])
                {
                    if((acc[indexLeft][indexRight] > maxAcc[indexLeft/3][indexRight/3])
                            || (indexLeft + indexRight < ensamble[indexLeft/3][indexRight/3][0]+ensamble[indexLeft/3][indexRight/3][1]))
                    {
                        maxAcc[indexLeft/3][indexRight/3] = acc[indexLeft][indexRight];
                        ensamble[indexLeft/3][indexRight/3][0] = indexLeft;
                        ensamble[indexLeft/3][indexRight/3][1] = indexRight;
                    }
                }
            }
        }


        //testiraj
        for(int k = 0; k < folds; k++)
        {
            System.out.println("FOLD " + (k+1));
            int datasize = dataSet[0][0].testCV(folds, k).numInstances();

            Classifier[][] ensambleScheme = new Classifier[3][3];
            Instances[][] testData = new Instances[3][3];

            //treniraj
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    int indexLeft = ensamble[i][j][0];
                    int indexRight = ensamble[i][j][1];

                    System.out.println("learning ensamble element: " + windowSizes[indexLeft] + "x" + windowSizes[indexRight]);

                    ensambleScheme[i][j] = classifiers[indexLeft][indexRight];
                    Instances data = dataSet[indexLeft][indexRight];
                    Instances trainSet = data.trainCV(folds, k);
                    testData[i][j] = data.testCV(folds, k);
                    ensambleScheme[i][j].buildClassifier(trainSet);
                }
            }

            //testiraj
            for(int l = datasize / 2; l < datasize; l++)
            {
                int[] votes = new int[dataSet[0][0].numClasses()];
                int trueOut = -1;

                for(int i = 0; i < 3; i++)
                {
                    for(int j = 0; j < 3; j++)
                    {
                        //System.out.println("classifing ...");

                        Classifier scheme = ensambleScheme[i][j];
                        Instances data = testData[i][j];
                        Instance inst = data.instance(randomSequence[k][l]);
                        int out = (int) scheme.classifyInstance(inst);

                        if(trueOut != -1 && trueOut != (int) inst.classValue()) throw new Exception("trueOut != inst.classValue()");
                        trueOut = (int)inst.classValue();

                        votes[out]++;
                    }
                }

                int out = findMax(votes);
                double val = cm.getElement(trueOut, out) + 1.0;
                cm.setElement(trueOut, out, val);
            }

        }

        DecimalFormat df = new DecimalFormat("#.##");
        for(int i = 0; i <9; i++)
        {
            for(int j = 0; j <9; j++)
                System.out.print(df.format(acc[i][j]) + "  ");
            System.out.println();
        }

    }

    private int findMax(int[] votes)
    {
        int max = -1;
        int id = -1;
        for(int i = 0; i < votes.length; i++)
        {
            if(votes[i]>max){
                max = votes[i];
                id = i;
            }
        }
        return id;
    }

    private ConfusionMatrix validate(Classifier scheme, Instances valTestSet, int start, int end, int[] randomSequence)
    {
        ConfusionMatrix m = new ConfusionMatrix(stringClasses);
        try
        {

            for(int i = start; i < end; i++)
            {
                int k = randomSequence[i];
                Instance inst = valTestSet.instance(k);
                int out = (int) scheme.classifyInstance(inst);

                int trueOut = (int) inst.classValue();
                double val = m.getElement(trueOut, out) + 1.0;

                m.setElement(trueOut, out, val);
            }
        }
        catch(Exception e)
        {
            e.printStackTrace();
        }

        return m;
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

    private void classify()
    {
        //train(trainSet);
        try{
            test();
        }
        catch(Exception e)
        {
            e.printStackTrace();;

        }
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

        classify();



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
