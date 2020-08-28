﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Shared.Integration;
using Shared.Integration.Models;

namespace Shared.Tests.CreditCard
{
    public class CreditCardTests
    {
        [Fact]
        public void CardVendorResolverTest()
        {
            var testData = @"
513771
557447
481582
403784
400344
458048
003655
479087
456188
525500
452002
497208
459440
510805
437851
456331
003620
518955
535522
457037
403864
530101
492910
474165
470881
411773
401057
510046
458099
535132
518941
493532
458097
548696
459664
432630
431906
548673
003640
552475
549252
497650
458094
437773
458000
464018
532611
458021
547854
548438
444055
540719
523400
554983
477915
522661
553691
548347
426690
408540
534244
458009
542418
555888
420767
414718
458007
476365
516815
552177
522078
471527
498003
525477
414780
529810
540501
539123
458029
458090
552479
424631
402199
458036
458091
458004
542432
525475
421718
497759
455743
481583
557281
524756
490070
535584
424689
497388
453747
471294
526229
456997
547532
544036
452088
418021
497992
546657
497856
518986
538404
003695
458080
550018
497040
449353
473702
467277
426398
552869
533491
456225
518954
458043
544434
455286
548901
432020
410039
552176
513381
441770
552213
458089
488893
492912
475129
552486
557435
513781
515590
513283
499897
414709
516499
544677
524038
442596
520416
465946
458010
415417
552449
554505
408397
422093
555947
552184
543700
465941
405911
493831
414720
449465
458016
458019
499839
516310
454617
402167
406068
478986
465943
521857
483950
533161
601100
458001
400612
530991
511891
559565
432264
419268
417903
412451
545136
483316
492915
533594
519931
458522
676663
523253
439707
465950
411776
414909
535316
441104
557914
526219
553963
519955
553295
407728
458052
497402
425704
414778
535120
442742
536115
543034
527346
516306
497600
465838
534109
458024
548346
535585
458082
601120
426684
558331
530770
537590
536229
559240
492010
546630
440210
531000
000000
462722
443041
456480
490117
536829
557098
453979
477548
543661
531170
456353
523254
468762
558158
407941
419266
486732
554906
523231
400273
531260
497202
547707
497047
553906
401849
402620
559591
407517
518791
458027
490071
451751
458643
540153
486742
428082
458011
510049
448975
449352
469279
486485
423580
400915
414730
497301
459632
518989
497278
438857
528683
407530
528939
532618
458030
448448
422002
453903
458058
558721
525610
539132
552492
003608
406049
458096
489452
427638
405479
454642
552517
426354
527616
540659
551898
491342
416081
414740
425125
549701
458086
465944
531423
536219
489396
510859
458006
411911
520953
526218
453997
546930
426113
536406
473077
442644
435546
549863
547415
427655
458041
513263
493000
550230
537591
421156
454033
528287
516872
428259
458028
535298
458053
549460
427938
400159
441103
491889
446282
531387
458546
549006
462263
466248
486348
551029
458049
522725
540187
537541
535590
458037
472409
440768
542179
458083
555949
532614
529204
513414
557907
517805
524886
559309
470403
552433
475117
431307
522660
455206
526430
425788
458015
458023
480465
459654
534875
455225
494024
448237
513162
498407
414734
433719
552183
446540
525615
451757
514940
432466
532180
519840
535687
481779
535838
496611
548236
497490
510197
480990
480715
457155
530115
421915
497401
535583
413575
433993
514759
476367
448796
474481
515676
434256
462239
548401
436618
512230
515241
545134
447597
546638
531719
457144
518542
442174
405071
457179
547685
532610
458017
544038
546540
458003
465901
457098
458059
492556
546811
419265
450231
444978
535470
433830
440066
524347
483312
537434
546616
516798
544990
455709
492181
497288
453826
530072
458008
489114
533669
458020
497542
498406
455744
458093
410040
553680
458012
427367
457006
456468
470724
541557
435044
420768
438854
470406
458047
546938
427082
422211
430579
518991
547718
458056
523252
402944
465583
460332
497804
549001
497291
530039
448233
403003
455148
511081
474476
406589
549703
491463
545198
479004
458098
458039
557523
533833
465858
513778
405621
552033
513659
451401
539259
521105
423568
540226
549004
474843
518953
510008
458088
478200
458014
475131
458042
525303
554267
423223
532612
483439
523255
458026
414949
521324
523224
543458
532954
535640";

            Dictionary<CardVendorEnum, int> res = new Dictionary<CardVendorEnum, int>();

            foreach (var bin in testData.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            {
                var vendor = bin.GetCardVendor();

                if (res.ContainsKey(vendor))
                {
                    res[vendor]++;
                }
                else
                {
                    res.Add(vendor, 1);
                }
            }

            Assert.NotNull(res);
        }
    }
}
