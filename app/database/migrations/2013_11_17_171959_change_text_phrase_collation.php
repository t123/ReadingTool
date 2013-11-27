<?php

use Illuminate\Database\Migrations\Migration;

class ChangeTextPhraseCollation extends Migration {

    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up() {
        DB::statement('alter table terms modify phrase varchar(50) character set utf8 collate utf8_bin');
        DB::statement('alter table terms modify phraseLower varchar(50) character set utf8 collate utf8_bin');
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        DB::statement('alter table terms modify phrase varchar(50) character set utf8 collate utf8_unicode_ci');
        DB::statement('alter table terms modify phraseLower varchar(50) character set utf8 collate utf8_unicode_ci');
    }

}
