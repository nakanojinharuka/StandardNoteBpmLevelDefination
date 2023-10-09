# Standard Note-Bpm Level Defination
A logarithm to analyze the chart from Bestdori.com which has upgraded 12 generations.
The description of the logarithm will be written by LaTeX format. Enjoy!
If irregular context I reccomend transform it to LaTeX to present, such as matrices.
(I have no idea about the irregularity anyway....)
# Major Text

This article, which contains a large component of advanced mathematics and algebra basis, is suitable for whom has an accountable foundation on this. In order to cut the scale of article, basic advanced mathematics and linear algebra items are not provided in the major content. If an elementary request is solidly purposed, we can consider to present some items.

# Overall Process

## Overall Suggestion

No matter what color the element flies on a specific chart, either blue, green, or white, it is rational that to charge every item as an object that it brought a tremendous amount of convenience in data gathering and generating. Comprehensively considering all the factors determining a chart’s level, we argues that it is how real-hit and screen-hit sharply varies in a differentiate second that it straightly contribute to what figure ought to be about the instance since the eighth generation of empirical equation. Owing to differentiation existence, we sequentially added integration from the ninth generation. Compared to the first three generation and that of the next four, either mechanically transplant parameters to puzzle a result or blindly use integration overlooked differentiation to address meaningless calculation, this equation adequately considered how tiny changes involve final result as well as self-correlation on both differentiation of real-hit and of screen-hit, which use difference rather than differentiation to scale down the complexity of the formula.

## Principal Component Analysis

How could the equation takes a principal component analysis in evaluation? However, it is necessary for us to explain why needed in the equation. A PCA on a couple of data presented in a matrix of covariance functions by figuring its eigenvalues and eigenvectors then resulting in variables which is less then raw variables’ amount and in which expressed by linear combination of source variables.

## Main Procedure

Above all, the twelfth generation is consisted of a series of progresses. Here are the procedures description:

-   Build a matrix using real hit and screen hit per second: This is a matrix which is arranged by special order and real symmetric type so that when execute eigenvalue decomposition the result is all real, instead of complex.

-   Eigenvalue decomposition: The decomposition determines what situation of a chart can be described by its value. This type of decomposition only used once on seek positive and negative inertia index in a 3rd order matrix.

-   Singular value decomposition: However, eigenvalues of a square real symmetric matrix locate on the whole district of **R**, while singular value can minimize the district to (0,+∞). Thanks to the specific theorem on real symmetric matrix that it is possible to use eigenvalues equal to singular-values to substitute minus-value-existence eigenvalue decomposition. It is so precious that it brings much more convenience on principal component analysis.

-   Equivalent value calculation by principal component analysis: Analyzing principal component can lower the dimension in linear combination. Linear combination matrix is the major result. In the data of the model, usually the time of a single chart is below 324 seconds, which is equal to 18 square. And the matrix is always full rank.

-   Linear regression twice make it to result: initially we consider equivalent value, and last positive inertia index, negative inertia index.


Now that the brief process is presented, it is time for generating major functions to evaluate. Nevertheless, the data we get rooting on two websites—-[bestdori.com](bestdori.com) and [ayachan.fun](ayachan.fun)—-are too irregular to analyze. It is of importance to normalize them. But how to normalize, how to conduct PCA analysis, and what coefficients we are going to use about linear regression? The next sections could provide you our answers. Before the article precedes, please make sure to have a solid and reliable soil or environment on calculus and linear algebra.

# Arrange and vary symmetric matrices

## Raw data handling

Two websites represents two servers as well as two databases that there is no need to paste them in that the stream limits of the two sites are so low that if there are huge requests occurs, both sites must start up security system. Such demand on presenting raw data that wrote like: $"Distribution":0,2,6,9,4,8,3,1,5,4,4,6,2,4,6,2, "Distribution":0,3,7,12,5,9,3,1,7,6,2,5,9,4,10,3$. Every chart there must exist a 0 on the first element. To solve the 0, just alter it to 1. And the two matrices we proposed are like this:
$$\begin{aligned}
RH=\begin{bmatrix}
1 & 2 & 4 & 4\\
6 & 9 & 3 & 6\\
8 & 1 & 5 & 4\\
4 & 2 & 6 & 2
\end{bmatrix},
SH=\begin{bmatrix}
 1 & 3 & 5 & 6\\
 7 & 12 & 3 & 5\\
 9 & 1 & 7 & 4\\
 2 & 9 & 10 & 3
\end{bmatrix}\tag{aa}\end{aligned}
$$

Equation <a href="#aa" data-reference-type="eqref" data-reference="aa">[aa]</a> respectively the real hit matrix and the screen hit matrix. But to make symmetric, we had better transform them as written below:
$$\begin{aligned}
R=RHS=\begin{bmatrix}
 1 & 4 & 6 & 4\\
 4 & 9 & 2 & 4\\
 6 & 2 & 5 & 5\\
 4 & 4 & 5 & 2
\end{bmatrix},
S=SHS=\begin{bmatrix}
 1 & 5 & 7 & 4\\
 5 & 12& 2 & 7\\
 7 & 2 & 7 & 7\\
 4 & 7 & 7 & 3
\end{bmatrix}\tag{ab}\end{aligned}$$

They are so magnificent that we can guarantee their eigenvalues are reals, indirectly resulting in their real singular values. How beautiful the matrices <a href="#ab" data-reference-type="eqref" data-reference="ab">[ab]</a> are!

In fact, reality situation is this below. We believe it suffers because normal cases we can literally deal with utmost 4 dimensions’ matrix. To avoid the heavy massive process, MathNet Library shows its strengths. Matrix <a href="#ae" data-reference-type="eqref" data-reference="ae">[ae]</a> can be decomposed by its function calls "Decomposition.Svd()". There is no such idea to explain how singular value decomposition executes, so that we ought to consequently write the result of decomposition as <a href="#af" data-reference-type="eqref" data-reference="af">[af]</a>.
$$\begin{aligned}
R&=\begin{bmatrix}
9&8&8&2&2&5&7&6&5&3\\
8&6&2&6&8&8&5&1&2&9\\
8&2&9&4&1&7&6&1&4&6\\
2&6&4&6&5&5&7&2&8&9\\
2&8&1&5&1&9&1&8&1&8\\
5&8&7&5&9&4&8&5&8&6\\
7&5&6&7&1&8&9&9&2&3\\
6&1&1&2&8&5&9&6&5&6\\
5&2&4&8&1&8&2&5&8&7\\
3&9&6&9&8&6&3&6&7&3
\end{bmatrix}\tag{ae, af}\\
\Lambda&=\begin{bmatrix}
54.2&&&&&&&&&\\
&12.4&&&&&&&&\\
&&12.4&&&&&&&\\
&&&10.5&&&&&&\\
&&&&9.38&&&&&\\
&&&&&7.68&&&&\\
&&&&&&5.91&&&\\
&&&&&&&4.41&&\\
&&&&&&&&2.38&\\
&&&&&&&&&0.71
\end{bmatrix}\end{aligned}$$

## PCA: Analyzing and rebinding new variables

Since eigenvalue decomposition provides orthogonal matrices <a href="#ag" data-reference-type="eqref" data-reference="ag">[ag]</a>, singular-value decomposition similar to this. Owing to <a href="#ac" data-reference-type="eqref" data-reference="ac">[ac]</a>, we can figure out <a href="#ad" data-reference-type="eqref" data-reference="ad">[ad]</a>, then we have <a href="#ah" data-reference-type="eqref" data-reference="ah">[ah]</a> and <a href="#ai" data-reference-type="eqref" data-reference="ai">[ai]</a> while *η* means singular value.
$$\begin{aligned}
R&=P_{1}^{-1}\Lambda P_{1}=Q_{1}^{T}\Lambda Q_{1}\left(Q^{T}=Q^{-1}\right)\\
V^{T}&=U\sim Q\\
\left|\lambda_{A}\right|&=\eta_{A}\\
R&=U_{1}\Theta V_{1}^{T}\\
S&=U_{2}\Omega V_{2}^{T}\tag{ag,ac,ad,ah,ai}
\end{aligned}$$

We take 634 charts from official for reference and use SPSS on principal component analysis. For real hit there is a transformation matrix below, while also for screen hit transformation matrix below:
$$\begin{aligned}
A&=\begin{bmatrix}
0.573822412 & 0.403300461 & 0.21830206 & 0.363258335 & 0.314142651  \\
0.495012493 & 0.526229742 & 0.366154776 & 0.197003012 & 0.317336326  \\
0.600070736 & 0.538714495 & 0.32456082 & 0.131691284 & -0.049297755  \\
0.667378867 & 0.487494182 & 0.278358937 & -0.029455065 & -0.249130597  \\
0.745488887 & 0.452260276 & 0.144290533 & -0.132603143 & -0.243176139  \\
0.766027932 & 0.438707807 & 0.012064089 & -0.207427336 & -0.197630758  \\
0.793629294 & 0.370828564 & -0.125409951 & -0.221536939 & -0.075812098  \\
0.777178421 & 0.323992546 & -0.286540778 & -0.215036556 & 0.062136586  \\
0.788119019 & 0.187119304 & -0.374563362 & -0.138527052 & 0.212881338  \\
0.769888501 & 0.094536572 & -0.444208968 & -0.008911905 & 0.228814824  \\
0.785203393 & -0.107104024 & -0.380002253 & 0.153941644 & 0.084210939  \\
0.825363523 & -0.290104522 & -0.246551914 & 0.239119223 & -0.137020242  \\
0.842317907 & -0.441687462 & 0.011751254 & 0.202582624 & -0.162244019  \\
0.829857328 & -0.445695131 & 0.012278047 & 0.209289416 & -0.171636976  \\
0.816988717 & -0.498883253 & 0.065483914 & 0.119493082 & -0.102762235  \\
0.769727611 & -0.543310787 & 0.223925361 & -0.064583304 & 0.03616166  \\
0.678703809 & -0.554105256 & 0.321390753 & -0.229660224 & 0.145421244  \\
0.580397108 & -0.538932076 & 0.33314101 & -0.350107981 & 0.245693312 
\end{bmatrix}\\
B&=\begin{bmatrix}
0.581006155 & 0.414589756 & 0.24564148 & 0.474641982 & 0.0789635  \\
0.510050593 & 0.568899439 & 0.340660459 & 0.292000976 & 0.120434541  \\
0.621610715 & 0.542724103 & 0.302564697 & 0.12249237 & -0.089594809  \\
0.701243943 & 0.490033566 & 0.179986681 & -0.143471062 & -0.174155259  \\
0.772989954 & 0.430841038 & 0.10335296 & -0.264794712 & -0.102243406  \\
0.763914635 & 0.44204174 & 0.023456496 & -0.302138557 & -0.068959401  \\
0.803969328 & 0.374135828 & -0.109371525 & -0.244218343 & 0.024212672  \\
0.817085723 & 0.234088248 & -0.266854486 & -0.127251308 & 0.116247665  \\
0.808578065 & 0.177051467 & -0.389749087 & 0.000904864 & 0.152749227  \\
0.79347482 & 0.034394834 & -0.436611073 & 0.128488569 & 0.162619503  \\
0.80715108 & -0.076871007 & -0.354446597 & 0.187327027 & 0.122304899  \\
0.834097781 & -0.309297879 & -0.188117513 & 0.151183932 & -0.127806728  \\
0.83663009 & -0.471136019 & 0.018104028 & 0.059151227 & -0.215986373  \\
0.824107803 & -0.488522137 & 0.038975386 & 0.067481703 & -0.2236268  \\
0.812272159 & -0.506676353 & 0.092817358 & 0.048997248 & -0.146074603  \\
0.805119101 & -0.518014943 & 0.156888708 & -0.001453936 & -0.076154375  \\
0.672075884 & -0.480579175 & 0.332848271 & -0.133497811 & 0.237117348  \\
0.559481947 & -0.494877337 & 0.374113306 & -0.175919479 & 0.408793939 
\end{bmatrix}\end{aligned}$$

For calculation, we should use process below:
$$\begin{aligned}
\eta _{he}=A^{T}\eta _{h}\\
\eta _{se}=B^{T}\eta _{s}
\end{aligned}$$

## Linear Regression to raw and precise adjustment

To *η*<sub>*he*</sub> and *η*<sub>*se*</sub> there are consistently 5 parameters. Linear regression is the best way to find correlations, however, with different vectors so that presenting the result makes nonsense. We continue to progress. Greek letter *η* writes E as its upper case. The matrices *L* each of which is line vector. If that is not straight, we can transpose them as <a href="#aj" data-reference-type="eqref" data-reference="aj">[aj]</a> and <a href="#al" data-reference-type="eqref" data-reference="al">[al]</a> convey. Frankly, there is no such other clear way to express that with brief equations. We compared the two values they evaluate and suggested that we should make a pair of proportion indexes, called raw adjustment, for adapting figures. We finally use 0.3 and 0.7 to balance, as the equation <a href="#ak" data-reference-type="eqref" data-reference="ak">[ak]</a> says. Sample form is beneath the formulas.
$$\begin{aligned}
k_{he}^{T}\eta _{he}&=l_{he}\\
k_{se}^{T}\eta _{se}&=l_{se}\\
k_{he}^{T}
\begin{bmatrix}
\eta _{he1} & \eta _{he2} & \eta _{he3} & \cdots & \eta _{hen}
\end{bmatrix}
&=
\begin{bmatrix}
l_{he1} & l_{he2} & l_{he3} & \cdots & l_{hen}
\end{bmatrix}\\
k_{he}^{T}E_{he}&=L_{he}^{T}\\
k_{se}^{T}
\begin{bmatrix}
\eta _{se1} & \eta _{se2} & \eta _{se3} & \cdots & \eta _{sen}
\end{bmatrix}
&=
\begin{bmatrix}
l_{se1} & l_{se2} & l_{se3} & \cdots & l_{sen}
\end{bmatrix}\\
k_{se}^{T}E\_{se}&=L\_{se}^{T}\\
L_{he}&=E_{he}^{T}k_{he}\\
L_{se}&=E_{se}^{T}k_{se}\\
R&=0.3L_{he}+0.7L_{se}\tag{aj,ai,ak}\end{aligned}$$
<style>
.center 
{
  width: auto;
  display: table;
  margin-left: auto;
  margin-right: auto;
}
</style>
<div class="center">

|   ID   |     Evaluation     | Original level |
|:------:|:------------------:|:--------------:|
| 117754 | 31.715530830491105 |       30       |
| 117002 | 28.72949664829032  |       28       |
| 109547 | 30.05948708500512  |       28       |
| 125651 | 27.275950541953048 |       29       |
| 118154 | 30.167339148022588 |       35       |
| 119679 |  25.0984658848988  |       26       |
| 114195 | 29.002161126552476 |       28       |
| 125288 | 25.374303445898732 |       25       |
| 123246 | 27.168420242436664 |       27       |
| 118955 | 26.93133457729617  |       27       |
| 67943  | 25.38588328795224  |       26       |

</div>
