/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package lib;

/**
 *
 * @author Ante
 */
import weka.classifiers.*;
import weka.classifiers.functions.*;
import weka.core.*;
import weka.classifiers.evaluation.ConfusionMatrix;
import java.util.*;
import javax.swing.*;
import libsvm.svm;
import libsvm.svm_print_interface;
import java.text.DecimalFormat;

public class SVM extends Skelet {
    public SVM()
    {
        int seed = new Random().nextInt();
        rand = new Random(seed);   // create seeded number generator
    }

    public void foo()
    {
        try{
            for(int indexLeft = 0; indexLeft < numOfWindowsSizes; indexLeft++)
            {
                for(int indexRight = 0; indexRight < numOfWindowsSizes; indexRight++)
                {
                    dataLocations[indexLeft][indexRight] = "D:\\arff\\interest\\interest" +windowSizes[indexLeft]+"x"+windowSizes[indexRight]+".arff";
                }
            }

            setDataLocations(dataLocations);

            validate();
        }
        catch(Exception e)
        {
            e.printStackTrace();
        }
    }

    public void validate() throws Exception
    {
        svm_print_interface newPrint = new svm_print_interface()
        {
                public void print(String s)
                {
                }
        };
        svm.svm_set_print_string_function(newPrint);

        makeRandomSequence();

        double[][] errors = new double[9][9];
        double[][] matrixC = new double[9][9];
        //int indexLeft = 5;
        //int indexRight=5;
        for(int indexLeft = 0; indexLeft < numOfWindowsSizes; indexLeft++)
        {
           for(int indexRight = 0; indexRight < numOfWindowsSizes; indexRight++)
            {
                Instances data = dataSet[indexLeft][indexRight];

                double C = 1.0;
                double step = 0.5;
                double threshold = 0.1;
                double foot = 2.0;
                double tolerance = 0.0001;

                ConfusionMatrix centralM = null;

                while((C < 2 && step >= threshold) ||(C >= 2 && step >= threshold*5))
                {
                    double leftC  = C - step;
                    double rightC = C + step;

                    while(leftC < 0.1){
                        step = step / foot;
                        leftC  = C - step;
                        rightC = C + step;
                    }
                    println("C: " +leftC +" " + C + " " + rightC);

                    ConfusionMatrix leftM;
                    ConfusionMatrix rightM;

                    centralM = testC(data, C);
                    leftM = testC(data, leftC);
                    rightM = testC(data,rightC);

                    println(leftM.errorRate()+ " "+ centralM.errorRate() + " " +rightM.errorRate());

                    if(rightM.errorRate() - centralM.errorRate() > tolerance) C = leftC;
                    else if (leftM.errorRate() - centralM.errorRate() > tolerance) C = rightC;
                    else step = step / foot;
                }

                errors[indexLeft][indexRight] = centralM.errorRate();
                matrixC[indexLeft][indexRight] = C;
                println("Size: "+windowSizes[indexLeft]+"x"+ windowSizes[indexRight]+" C: " + C);
            }
        }

        DecimalFormat df = new DecimalFormat("#.##");
        for(int i = 0; i <9; i++)
        {
            for(int j = 0; j <9; j++)
                System.out.print(df.format(errors[i][j]) + " - " +df.format(matrixC[i][j])+ "  ");
            System.out.println();
        }
    }

    public ConfusionMatrix testC(Instances data, double C) throws Exception
    {
        ConfusionMatrix m = new ConfusionMatrix(classesToString());

        for(int k = 0; k < folds; k++)
        {

            Instances trainSet = data.trainCV(folds, k);
            LibSVM scheme = new LibSVM();

            scheme.setCost(C);
            scheme.setKernelType(new SelectedTag( LibSVM.KERNELTYPE_LINEAR, LibSVM.TAGS_KERNELTYPE ));
            //String[] optSVR = weka.core.Utils.splitOptions("-S 0 -H 0 -K 0 -D 3 -G 0.0 -R 0.0 -N 0.5 -M 40.0 -C 15.0 -E 0.0010 -P 0.1 -q 0");
                
            scheme.buildClassifier(trainSet);

            Instances testSet = data.testCV(folds, k);
            for(int l = 0; l < testSet.numInstances()/2; l++)
            {
                Instance inst = testSet.instance(getIndex(k,l));
                int out = (int) scheme.classifyInstance(inst);

                int trueOut = (int)inst.classValue();
                incConfusionMatrix(m, trueOut, out);
            }
        }

        return m;
    }

}
