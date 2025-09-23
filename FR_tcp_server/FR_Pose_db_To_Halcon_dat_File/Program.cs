using FR_Pose_db_To_Halcon_dat_File;

PoseToDat.ExportDbToDat(
    @"E:\work\FutureRay\FR_Robot\GIT\FR_tcp_server\FR_Pose_db_To_Halcon_dat_File\point_table_cal.db",
    PoseToDat.GetValidatedSavePath("datFile")
    );